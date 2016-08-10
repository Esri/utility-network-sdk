//   Copyright 2016 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.Data.UtilityNetwork;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Events;
using UtilityNetworkSamples;

namespace LoadReportSample
{
  /// <summary>
  /// This addin demonstrates the creation of a simple electric distribution report.  It traces downstream from a given point and adds up the count of customers and total load per phase.  This sample
  /// is meant to be a demonstration on how to use the Utility Network portions of the SDK.  The report display is rudimentary.  Look elsewhere in the SDK for better examples on how to display data.
	/// 
	/// Rather than coding special logic to pick a starting point, this sample leverages the existing Set Trace Locations tool that is included with the product.
	/// That tool writes rows to a table called UN_Temp_Starting_Points, which is stored in the default project workspace.  This sample reads rows from that table and uses them as starting points
	/// for our downstream trace.
  /// </summary>
  /// <remarks>
  /// Instructions for use
  /// 1. Select a feature layer that participates in a utility network
  /// 2. Click on the SDK Samples tab on the Utility Network tab group
  /// 3. Click on the Starting Points tool to create a starting point on the map
  /// 4. Click on the Create Load Report tool
  /// </remarks>

  internal class CreateLoadReport : Button
  {
    // Constants - used with the starting points table created by the Set Trace Locations tool

    private const string StartingPointsTableName = "UN_Temp_Starting_Points";
    private const string StartingPointsGuidFieldName = "FEATUREGLOBALID";
    private const string StartingPointsTerminalFieldName = "TERMINALID";

		// Constants - used with the Esri Electric Distribution Data Model

    private const string ElectricDistributionDeviceTableName = "ElectricDistributionDevice";
    private const string ServicePointSubtypeName = "ServicePoint";
    private const string PhasesFieldName = "PHASESNORMAL";
    private const string LoadFieldName = "SERVICECURRENTRATING";
    private const string SubtypeNetworkAttributeName = "Asset group";


		/// <summary>
		/// OnClick
		/// 
		/// This is the implementation of our button.  We pass the selected layer to GenerateReport() which does the bulk of the work.
		/// We then display the results, along with error messages, in a MessageBox.
		/// 
		/// </summary>
		
    protected override void OnClick()
    {
			// Start by checking to make sure we have a single feature layer selected

      if (MapView.Active == null)
      {
        MessageBox.Show("Please select a feature layer that participates in a utility network.", "Create Load Report");
        return;
      }

      MapViewEventArgs mapViewEventArgs = new MapViewEventArgs(MapView.Active);
      if (mapViewEventArgs.MapView.GetSelectedLayers().Count != 1 || !(mapViewEventArgs.MapView.GetSelectedLayers()[0] is FeatureLayer))
      {
        MessageBox.Show("Please select a feature layer that participates in a utility network.", "Create Load Report");
        return;
      }

      // Currently this tool does not work if the selected layer is the UtilityNetworkLayer itself.  The SDK will eventually be extended so that we can return the utility network object from the 
      // UtilityNetworkLayer.

      if (mapViewEventArgs.MapView.GetSelectedLayers()[0] is UtilityNetworkLayer)
      {
        MessageBox.Show("Please select a feature layer that participates in a utility network.", "Create Load Report");
        return;
      }

			// Generate our report.  The LoadTraceResults class is used to pass back results from the worker thread to the UI thread that we're currently executing.

      Task<LoadTraceResults> task = GenerateReport(mapViewEventArgs.MapView.GetSelectedLayers()[0] as FeatureLayer);
      LoadTraceResults traceResults = task.Result;

			// Assemble a string to show in the message box

      string traceResultsString;
      if (traceResults.Success)
      {
        traceResultsString = String.Format("Customers per Phase:\n   A: {0}\n   B: {1}\n   C: {2}\n\nLoad per Phase:\n   A: {3}\n   B: {4}\n   C: {5}\n\n{6}",
          traceResults.NumberServicePointsA.ToString(), traceResults.NumberServicePointsB.ToString(), traceResults.NumberServicePointsC.ToString(),
          traceResults.TotalLoadA.ToString(), traceResults.TotalLoadB.ToString(), traceResults.TotalLoadC.ToString(),
          traceResults.Message);
      }
      else
      {
        traceResultsString = traceResults.Message;
      }

			// Show our results

      MessageBox.Show(traceResultsString, "Create Load Report");
    }


		/// <summary>
		/// GenerateReport
		/// 
		/// This routine takes a feature layer that references a feature class that participates in a utility network.
		/// It returns a set of data to display on the UI thread.
		/// 
		/// 
		/// </summary>

		public Task<LoadTraceResults> GenerateReport(FeatureLayer utilityNetworkFeatureLayer)
		{

			// This is the standard way in Pro to execute a set of code on the MCT (worker) thread.  Note that many of these routines can ONLY be called on the MCT thread.

			return QueuedTask.Run<LoadTraceResults>(() =>
			{

				// Create a new results object.  We use this class to pass back a set of data from the worker thread to the UI thread

				LoadTraceResults results = new LoadTraceResults();

				// Initialize a number of geodatabase objects

				using (Geodatabase utilityNetworkGeodatabase = utilityNetworkFeatureLayer.GetFeatureClass().GetDatastore() as Geodatabase)
				using (FeatureClass electricDistributionDeviceFeatureClass = utilityNetworkGeodatabase.OpenDataset<FeatureClass>(ElectricDistributionDeviceTableName))
				using (FeatureClassDefinition electricDistributionDeviceDefinition = utilityNetworkGeodatabase.GetDefinition<FeatureClassDefinition>(ElectricDistributionDeviceTableName))
				using (UtilityNetwork utilityNetwork = UtilityNetworkUtils.GetUtilityNetworkFromGeodatabase(utilityNetworkGeodatabase))
				using (UtilityNetworkTopology utilityNetworkTopology = utilityNetwork.GetNetworkTopology())
				using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
				using (Geodatabase defaultGeodatabase = new Geodatabase(Project.Current.DefaultGeodatabasePath))
				{
          // Get a row from the starting points table in the default project workspace.  This table is created the first time the user creates a starting point
          // If the table is missing or empty, a null row is returned

          using (Row startingPointRow = GetStartingPointRow(defaultGeodatabase, ref results))
          {
            if (startingPointRow != null)
            {

              // Convert starting point row into network element

              NetworkElement startingPointNetworkElement = GetNetworkElementFromStartingPointRow(startingPointRow, utilityNetworkTopology);

              // Create analysis object

              ElectricAnalyst analyst = utilityNetwork.CreateAnalysis(UtilityNetworkDomainType.ElectricDistribution) as ElectricAnalyst;
              if (analyst == null)
              {
                results.Message += "Please choose an electric network.\n";
                results.Success = false;
                return results;
              }

              // Set our starting point

              List<NetworkElement> startingPointList = new List<NetworkElement>();
              startingPointList.Add(startingPointNetworkElement);
              analyst.AddStartingPoints(startingPointList);

              // Trace downstream from starting network element

              IReadOnlyList<NetworkElement> traceResults;
              try
              {
                traceResults = analyst.TraceDownstream();
              }
              catch (Exception)
              {
                results.Message += "Trace failed.  Please make sure to place on a starting point on a feature that belongs to a circuit.\n";
                results.Success = false;
                return results;
              }

              // Get a NetworkEvaluator to get the subtype from network elements in the resultset
              
              NetworkAttribute subtypeNetworkAttribute = UtilityNetworkUtils.GetNetworkAttributeByName(utilityNetworkDefinition, SubtypeNetworkAttributeName);

              // Get the network source ID for the ElectricDistributionDevice table

              long distributionDeviceSourceID = utilityNetworkDefinition.GetNetworkSource(ElectricDistributionDeviceTableName).ID;

              // Get the subtype code for ServicePoint

              IReadOnlyList<Subtype> subtypeList = electricDistributionDeviceDefinition.GetSubtypes();
              int servicePointSubtypeCode = UtilityNetworkUtils.GetSubtypeByName(electricDistributionDeviceDefinition, ServicePointSubtypeName).GetCode();

              // loop through the trace results, building a list of GlobalIDs of the service points that are returned from the trace
              // We first filter by junctions, then by feature class, then by subtype

              List<Guid> globalIDList = new List<Guid>();
              foreach (NetworkElement resultNetworkElement in traceResults)
              {
                if (resultNetworkElement.Type == ElementType.Junction)
                {
                  // Get feature element from network element and check the source ID. We only care about ElectricDisributionDevice features

                  FeatureElement resultFeatureElement = utilityNetworkTopology.GetFeatureElement(resultNetworkElement);
                  if (resultFeatureElement.NetworkSource.ID == distributionDeviceSourceID)
                  {
                    // Get subtype from the network element - we only care about features with the ServicePoint subtype

                    FieldEvaluator evaluator = utilityNetworkTopology.GetNetworkEvaluator(resultNetworkElement, subtypeNetworkAttribute) as FieldEvaluator;
                    object vResultSubtypeCode = evaluator.Value;
                    long resultSubtypeCode = (long)vResultSubtypeCode;
                    if (resultSubtypeCode == servicePointSubtypeCode)
                    {
                      // We've found a ServicePoint.  Add its GlobalID to our list of features to fetch

                      globalIDList.Add(resultFeatureElement.GlobalID);
                    }
                  }
                }
              }

              // Fetch the service point features to get the count and load per phase

              AccumulateDataFromServicePoints(utilityNetworkGeodatabase, electricDistributionDeviceFeatureClass, electricDistributionDeviceDefinition, globalIDList, ref results);
            }

            // append success message to the output string

            results.Message += "Trace successful.";
            results.Success = true;
          }
				}
				return results;
			});
		}


		/// <summary>
		/// GetStartingPointRow
		/// 
		/// This routine opens up the starting points table and tries to read a row.  This table is created in 
		/// the default project workspace when the user first creates a starting point.
		/// 
		/// If the table doesn't exist or is empty, we add an error to our results object a null row.
		/// If the table contains one row, we just return the row
		/// If the table contains more than one row, we return the first row, and log a warning message
		///		(this tool only works with one starting point)
		/// 
		/// </summary>

		private Row GetStartingPointRow(Geodatabase defaultGeodatabase, ref LoadTraceResults results)
		{
			try
			{
				using (FeatureClass startingPointsFeatureClass = defaultGeodatabase.OpenDataset<FeatureClass>(StartingPointsTableName))
				using (RowCursor startingPointsCursor = startingPointsFeatureClass.Search())
				{
					if (startingPointsCursor.MoveNext())
					{
            Row row = startingPointsCursor.Current;
						
						if (startingPointsCursor.MoveNext())
						{
							// If starting points table has more than one row, append warning message
							results.Message += "Multiple starting points found.  Only the first one was used.";
							startingPointsCursor.Current.Dispose();
						}
						return row;
						
					}
					else
					{
						// If starting points table has no rows, exit with error message
						results.Message += "No starting points found.  Please create one using the Set Trace Locations tool.\n";
						results.Success = false;
						return null;
					}
				}
			}
			// If we cannot open the feature class, an exception is thrown
			catch (Exception)
			{
				results.Message += "No starting points found.  Please create one using the Set Trace Locations tool.\n";
				results.Success = false;
				return null;
			}
		}


		/// <summary>
		/// GetNetworkElementFromStartingPoint
		/// 
		/// This routine takes a row from the starting point table and converts it to a NetworkElement that we can use for tracing
		/// 
		/// </summary>
		/// 

    private NetworkElement GetNetworkElementFromStartingPointRow(Row startingPointRow, UtilityNetworkTopology utilityNetworkTopology)
    {

      // Fetch the Guid, and TerminalID values from the starting point row

      object vGlobalID = startingPointRow[StartingPointsGuidFieldName];
      Guid globalID = new Guid(vGlobalID.ToString());

      object vTerminalID = startingPointRow[StartingPointsTerminalFieldName];
      int terminalID = (int)vTerminalID;

      // Given a GlobalID and TerminalID, we can get a NetworkElement from the index

      return UtilityNetworkUtils.GetNetworkElementFromGuidAndTerminalID(utilityNetworkTopology, globalID, terminalID);
    }


		/// <summary>
		/// AccumulateDataFromServicePoints
		/// 
		/// This routine takes a set of GlobalIDs representing service points.  We fetch those rows and add the loads and customer counts to our results
		/// 
		/// </summary>
		/// <remarks>
		///	
		/// We will be using an I clause to fetch ServicePoint features from the list of Guids.
		/// Oracle limits the number of elements that can be passed to a single IN clause.  This routine breaks up our initial Guid list into subsets and issues multiple queries to get around this limitation.
		/// 
		/// Future versions of the SDK will provide an easier way to accomplish this
		/// </remarks>

    private void AccumulateDataFromServicePoints(Geodatabase utilityNetworkGeodatabase, FeatureClass electricDistributionDeviceFeatureClass, FeatureClassDefinition electricDistributionDeviceDefintion, List<Guid> globalIDList, ref LoadTraceResults results)
    {

      const int SubsetSize = 1000;

      int startingIndex = 0;
      int remainingCount = globalIDList.Count;

      while (remainingCount > 0)
      {
        int numToCopy = System.Math.Min(remainingCount, SubsetSize);
        List<Guid> guidSubset = globalIDList.GetRange(startingIndex, numToCopy);

        FetchCountAndLoadPerPhase(utilityNetworkGeodatabase, electricDistributionDeviceFeatureClass, electricDistributionDeviceDefintion, guidSubset, ref results);

        startingIndex += numToCopy;
        remainingCount -= numToCopy;
      }
    }


		/// <summary>
		/// FetchCountAndLoadPerPhase
		/// 
		/// This routine builds our query to fetch rows from the ServicePoint feature class.  We then read the Phases and Load fields and use those to accumulate our results
		/// 
		/// </summary>
		
    private void FetchCountAndLoadPerPhase(Geodatabase utilityNetworkGeodatabase, FeatureClass electricDistributionDeviceFeatureClass, FeatureClassDefinition electricDistributionDeviceDefinition, List<Guid> globalIDList, ref LoadTraceResults results)
    {

      // If we have a empty list of Global IDs, we can exit now.

      if (globalIDList.Count == 0) 
			{
				return;
			}

      // build IN clause with a list of our global ids

      StringBuilder globalIDListStringBuilder = new StringBuilder(" IN (");
      bool first = true;
      foreach (Guid guid in globalIDList)
      {
        if (!first)
        {
          globalIDListStringBuilder.Append(", ");
        }
        globalIDListStringBuilder.AppendFormat("'{0}'", guid.ToString("B").ToUpper());
        first = false;
      }
      globalIDListStringBuilder.Append(")");

      // Create a query filter

      QueryFilter queryFilter = new QueryFilter
      {
        WhereClause = electricDistributionDeviceDefinition.GetGlobalIDField() + globalIDListStringBuilder.ToString()
      };

      // Fetch the features

      using (RowCursor rowCursor = electricDistributionDeviceFeatureClass.Search(queryFilter))
      {
        while (rowCursor.MoveNext())
        {
          using (Feature servicePointFeature = rowCursor.Current as Feature)
          {

            // get phase from service point

            object vPhase = servicePointFeature[PhasesFieldName];
            short phase = (short)vPhase;

            // Get load from service point - use "Service Current Rating" attribute
            // IMPORTANT NOTE: Our data model does not currently include this field.  Defaulting to 200A for now
            // object vLoad = servicePointFeature[LoadFieldName];
            // int load = (int)vLoad;

            int load = 200;

            // increase count and total load per phase

            if (IsPhaseEnabled(phase, APhase))
            {
              results.NumberServicePointsA++;
              results.TotalLoadA += load;
            }
            if (IsPhaseEnabled(phase, BPhase))
            {
              results.NumberServicePointsB++;
              results.TotalLoadB += load;
            }
            if (IsPhaseEnabled(phase, CPhase))
            {
              results.NumberServicePointsC++;
              results.TotalLoadC += load;
            }
          }
        }
      }
    }


		/// <summary>
		/// Phase bitmap
		/// 
		/// Our Phases field uses a bitmap to encode which phases are present.
		/// This routine and set of constants are used to check for the presence of a particular phase
		/// 
		/// </summary>

		private const short APhase = 2;
		private const short BPhase = 1;
		private const short CPhase = 0;

		private bool IsPhaseEnabled(short s, int pos)
		{
			return (s & (1 << pos)) != 0;
		}



  }
}


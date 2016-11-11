﻿//   Copyright 2016 Esri
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
using ArcGIS.Core.Internal.Data.UtilityNetwork.Trace;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
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
    private const string StartingPointsGlobalIDFieldName = "FEATUREGLOBALID";
    private const string StartingPointsTerminalFieldName = "TERMINALID";

		// Constants - used with the Esri Electric Distribution Data Model

    private const string ServicePointCategory = "ServicePoint";
    private const string DeviceStatusAttributeName = "Device Status";

    private const short DeviceStatusOpened = 0;
    private const short DeviceStatusClosed = 1;

    private const string PhasesAttributeName = "Phases Normal";
    private const string LoadAttributeName = "Load";

    private const short APhase = 4;
    private const short BPhase = 2;
    private const short CPhase = 1;

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

        using (FeatureClass utilityNetworkFeatureClass = utilityNetworkFeatureLayer.GetFeatureClass())
				using (UtilityNetwork utilityNetwork = UtilityNetworkUtils.GetUtilityNetworkFromFeatureClass(utilityNetworkFeatureClass))
				using (UtilityNetworkTopology utilityNetworkTopology = utilityNetwork.GetNetworkTopology())
				using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        using (Geodatabase defaultGeodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(Project.Current.DefaultGeodatabasePath))))
        using (TraceManager traceManager = utilityNetwork.GetTraceManager())
        {
          // Get a row from the starting points table in the default project workspace.  This table is created the first time the user creates a starting point
          // If the table is missing or empty, a null row is returned

          using (Row startingPointRow = GetStartingPointRow(defaultGeodatabase, ref results))
          {
            if (startingPointRow != null)
            {

              // Convert starting point row into network element

              NetworkElement startingPointNetworkElement = GetNetworkElementFromStartingPointRow(startingPointRow, utilityNetworkTopology);

              // Obtain a tracer object

              DownstreamTracer downstreamTracer = traceManager.GetTracer<DownstreamTracer>();

              // Get the network attributes that we will use in our trace

              NetworkAttribute phasesNetworkAttribute = utilityNetworkDefinition.GetNetworkAttribute(PhasesAttributeName);
              NetworkAttribute loadNetworkAttribute = utilityNetworkDefinition.GetNetworkAttribute(LoadAttributeName);
              NetworkAttribute deviceStatusNetworkAttribute = utilityNetworkDefinition.GetNetworkAttribute(DeviceStatusAttributeName);

              // Set up our traversal filters

              Filter aPhaseFilter = new NetworkAttributeFilter(phasesNetworkAttribute, FilterOperator.BitwiseAnd, APhase);
              Filter bPhaseFilter = new NetworkAttributeFilter(phasesNetworkAttribute, FilterOperator.BitwiseAnd, BPhase);
              Filter cPhaseFilter = new NetworkAttributeFilter(phasesNetworkAttribute, FilterOperator.BitwiseAnd, CPhase);

              // Create function to add up loads on service points

              Function sumServicePointLoadFunction = new Sum(loadNetworkAttribute);

              // Create trace configuration object

              TraceConfiguration traceConfiguration = new TraceConfiguration();
              traceConfiguration.TerminatorFilter = new NetworkAttributeFilter(deviceStatusNetworkAttribute, FilterOperator.Equal, DeviceStatusOpened);
              traceConfiguration.OutputCategories = new List<string>() { ServicePointCategory };
              traceConfiguration.Functions = new List<Function>() { sumServicePointLoadFunction };

              // Create starting point list and trace argument object

              List<NetworkElement> startingPointList = new List<NetworkElement>() { startingPointNetworkElement };
              TraceArgument traceArgument = new TraceArgument(startingPointList);

              // Execute the trace on A phase

              traceConfiguration.TraversalFilter = aPhaseFilter;
              traceArgument.Configuration = traceConfiguration;
              try
              {
                TraceResult resultsA = downstreamTracer.Trace(traceArgument);
                results.NumberServicePointsA = resultsA.TraceOutput.Count;
                if (resultsA.FunctionOutput.Count > 0)
                {
                  results.TotalLoadA = (double)resultsA.FunctionOutput.First().GlobalValue;
                }
              }
              catch (ArcGIS.Core.Data.GeodatabaseUtilityNetworkException e)
              {
                //No A phase connectivity to source
                if (!e.Message.Equals("No subnetwork source was discovered.") )
                {
                  results.Success = false;
                  results.Message += e.Message;
                }
              }

              // Execute the trace on B phase

              traceConfiguration.TraversalFilter = bPhaseFilter;
              traceArgument.Configuration = traceConfiguration;

              try
              {
                TraceResult resultsB = downstreamTracer.Trace(traceArgument);
                results.NumberServicePointsB = resultsB.TraceOutput.Count;
                if (resultsB.FunctionOutput.Count > 0)
                {
                  results.TotalLoadB = (double)resultsB.FunctionOutput.First().GlobalValue;
                }
              }
              catch (ArcGIS.Core.Data.GeodatabaseUtilityNetworkException e)
              {
                // No B phase connectivity to source
                if (!e.Message.Equals("No subnetwork source was discovered."))
                {
                  results.Success = false;
                  results.Message += e.Message;
                }
              }

              // Execute the trace on C phase

              traceConfiguration.TraversalFilter = cPhaseFilter;
              traceArgument.Configuration = traceConfiguration;
              try
              {
                TraceResult resultsC = downstreamTracer.Trace(traceArgument);
                results.NumberServicePointsC = resultsC.TraceOutput.Count;
                if (resultsC.FunctionOutput.Count > 0)
                {
                  results.TotalLoadC = (double)resultsC.FunctionOutput.First().GlobalValue;
                }
              }
              catch (ArcGIS.Core.Data.GeodatabaseUtilityNetworkException e)
              {
                // No C phase connectivity to source
                if (!e.Message.Equals("No subnetwork source was discovered."))
                {
                  results.Success = false;
                  results.Message += e.Message;
                }
              }
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

      object vGlobalID = startingPointRow[StartingPointsGlobalIDFieldName];
      Guid globalID = new Guid(vGlobalID.ToString());

      object vTerminalID = startingPointRow[StartingPointsTerminalFieldName];
      int terminalID = (int)vTerminalID;

      // Given a GlobalID and TerminalID, we can get a NetworkElement from the index

      return UtilityNetworkUtils.GetNetworkElementFromGuidAndTerminalID(utilityNetworkTopology, globalID, terminalID);
    }
  }
}


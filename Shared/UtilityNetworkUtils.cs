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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Internal.Data.UtilityNetwork;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.Mapping;

namespace UtilityNetworkSamples
{
  class UtilityNetworkUtils
  {

    /// <summary>
    /// GetUtilityNetworkFromFeatureClass - gets a utility network from a feature class
    /// </summary>
    /// <remarks>
    /// A feature class can belong to multiple controller datasets, but at most one of them will be a UtilityNetwork.
    /// </remarks>
    /// <param name="featureClass"></param>
    /// <returns>a UtilityNetwork object, or null if the feature class does not belong to a utility network</returns>
   
    public static UtilityNetwork GetUtilityNetworkFromLayer(Layer layer)
    {

      if (layer is UtilityNetworkLayer)
      {
        UtilityNetworkLayer utilityNetworkLayer = layer as UtilityNetworkLayer;
        return utilityNetworkLayer.GetUtilityNetwork();
      }

      else if (layer is FeatureLayer)
      {
        FeatureLayer featureLayer = layer as FeatureLayer;
        using (FeatureClass featureClass = featureLayer.GetFeatureClass())
        {
          if (featureClass.IsControllerDatasetSupported())
          {
            IReadOnlyList<Dataset> controllerDatasets = featureClass.GetControllerDatasets();
            foreach (Dataset controllerDataset in controllerDatasets)
            {
              if (controllerDataset is UtilityNetwork)
              {
                return controllerDataset as UtilityNetwork;
              }
            }
          }
        }
      }
      return null;
    }

    public static Row FetchRowFromFeatureElement(UtilityNetwork utilityNetwork, FeatureElement featureElement)
    {
      // Get the feature class from the feature element
      using (FeatureClass featureClass = utilityNetwork.GetFeatureClass(featureElement.NetworkSource as FeatureSource))
      using (FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition())
      {
        // Create a query filter to fetch the appropriate row
        QueryFilter queryFilter = new QueryFilter()
        {
          WhereClause = featureClassDefinition.GetGlobalIDField() + " = {" + featureElement.GlobalID.ToString().ToUpper() + "}"
        };

        // Fetch and return the row
        RowCursor rowCursor = featureClass.Search(queryFilter);
        if (rowCursor.MoveNext())
        {
          return rowCursor.Current;
        }
        return null;
      }

    }

    public static FeatureLayer FindFeatureLayer(IReadOnlyList<Layer> layerList, FeatureClass featureClassToFind, int subtypeCodeToFind)
    {
      foreach (Layer layer in layerList)
      {
        // If we find a FeatureLayer, check to see if it is the correct feature class
        if (layer is FeatureLayer)
        {
          FeatureLayer featureLayer = layer as FeatureLayer;
          if (IsCorrectFeatureClass(featureLayer, featureClassToFind))
          {
            return featureLayer;
          }
        }

        // If we find a SubtypeGroupLayer, iterate through the children to find the layer that corresponds to our subtype
        else if (layer is SubtypeGroupLayer)
        {
          CompositeLayer compositeLayer = layer as CompositeLayer;
          IReadOnlyList<Layer> subtypeLayers = compositeLayer.Layers;
          if (subtypeLayers.Count > 0)
          {
            FeatureLayer firstSubtypeLayer = subtypeLayers.First() as FeatureLayer;
            if (IsCorrectFeatureClass(firstSubtypeLayer, featureClassToFind))
            {
              foreach(Layer subtypeLayer in subtypeLayers)
              {
                FeatureLayer subtypeFeatureLayer = subtypeLayer as FeatureLayer;
                if (IsCorrectSubtype(subtypeFeatureLayer, subtypeCodeToFind))
                  return subtypeFeatureLayer;
              }
            }
          }
        }

        // If we find a different kind of composite layer, just call this routine recursively
        else if (layer is CompositeLayer)
        {
          CompositeLayer compositeLayer = layer as CompositeLayer;

          FeatureLayer foundFeatureLayer = FindFeatureLayer(compositeLayer.Layers, featureClassToFind, subtypeCodeToFind);
          if (foundFeatureLayer != null)
          {
            return foundFeatureLayer;
          }
        }
      }
      
      return null;
    }

    private static bool IsCorrectSubtype(FeatureLayer featureLayer, int subtypeCodeToFind)
    {
      CIMFeatureLayer cimFeatureLayer = featureLayer.GetDefinition() as CIMFeatureLayer;
      CIMFeatureTable cimFeatureTable = cimFeatureLayer.FeatureTable;
      return subtypeCodeToFind == cimFeatureTable.SubtypeValue;
    }

    private static bool IsCorrectFeatureClass(FeatureLayer featureLayer, FeatureClass featureClass)

    {
      return featureLayer.GetFeatureClass().GetName() == featureClass.GetName();
    }

    public static FeatureSource GetFeatureSourceByUsageType(DomainNetwork domainNetwork, FeatureClassUsageType usageType)
    {
      IReadOnlyList<NetworkSource> networkSources = domainNetwork.NetworkSources;
      foreach (NetworkSource networkSource in networkSources)
      {
        if (networkSource is FeatureSource)
        {
          FeatureSource featureSource = networkSource as FeatureSource;
          if (featureSource.FeatureClassUsageType == usageType)
          {
            return featureSource;
          }
        }
      }
      return null;
    }

    public static int FindSubtypeCodeWithCategory(FeatureSource featureSource, string category)
    {
      foreach(AssetGroup assetGroup in featureSource.GetAssetGroups())
      {
        if (AssetGroupSupportsCategory(assetGroup, category))
        {
          return assetGroup.Code;
        }
      }
      return -1;  //nothing found
    }

    private static bool AssetGroupSupportsCategory(AssetGroup assetGroup, string category)
    {
      foreach(AssetType assetType in assetGroup.GetAssetTypes())
      {
        if (assetType.CategoryList.Contains(category))
        {
          return true;
        }
      }
      return false;
    }
  }
}

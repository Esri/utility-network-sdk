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

    /// <summary>
    /// GetFeatureClassFromFeatureSource
    /// 
    /// In the current beta, the FeatureSource class provides a name property that returns the name of the underlying database table.  
    /// Unfortunately, when using feature services, the utility network tables are exposed using different names.
    /// While the underlying database table might be named "ElectricDistributionDevice", the feature class name exposed by the service might
    /// be "L4Electric_Distribution_Device".  It turns out that the alias of the feature class name matches the name of the
    /// underlying database table.  This utility function provides a workaround for this issue.
    /// 
    /// In particular, this routine can be used to fetch a feature from a FeatureElement that is returned from a network trace.
    /// 
    /// Future versions of the utility network API will provide a more elegant solution that does not require a map.
    /// </summary>
    /// <param name="map"></param>
    /// <param name="featureSource"></param>
    /// <returns></returns>
    public static FeatureClass GetFeatureClassFromFeatureSource(Map map, FeatureSource featureSource)
    {
      // Get the table name from the FeatureSource.  This is the name of the underlying database table.
      string databaseTableName = featureSource.Name;

      // Now walk through each FeatureLayer in the map.  
      IReadOnlyList<Layer> layerList = map.GetLayersAsFlattenedList();
      foreach (Layer layer in layerList)
      {
        if (layer is FeatureLayer)
        {
          // Now check to see if the feature class alias matches the name we are looking for
          FeatureLayer featureLayer = layer as FeatureLayer;
          FeatureClass featureClass = featureLayer.GetFeatureClass();
          FeatureClassDefinition featureClassDefinition = featureClass.GetDefinition();
          if (featureClassDefinition.GetAliasName() == databaseTableName)
          {
            return featureClass;
          }
        }
      }
      return null;
    }
  }
}

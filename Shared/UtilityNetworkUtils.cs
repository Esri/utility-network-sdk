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

          // FeatureClass.IsControllerDatasetSupported() is the preferred technique for getting a UtilityNetwork from a 
          // FeatureClass.  Unfortunately, this routine is not yet implemented for Feature Service Workspaces, so we need to use this workaround
          // (which will not work in the event that multiple utility networks exist in the same workspace)

          Geodatabase geodatabase = featureClass.GetDatastore() as Geodatabase;

          if (geodatabase.GetGeodatabaseType() == GeodatabaseType.Service)
          {
            IReadOnlyList<UtilityNetworkDefinition> listUtilityNetworkDefinitions = geodatabase.GetDefinitions<UtilityNetworkDefinition>();
            if (listUtilityNetworkDefinitions.Count == 1)
            {
              UtilityNetworkDefinition utilityNetworkDefinition = listUtilityNetworkDefinitions[0];
              UtilityNetwork utilityNetwork = geodatabase.OpenDataset<UtilityNetwork>(utilityNetworkDefinition.GetName());
              return utilityNetwork;
            }
          }
          else if (featureClass.IsControllerDatasetSupported())
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
  }
}

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

namespace UtilityNetworkSamples
{
  class UtilityNetworkUtils
  {
    // GetUtilityNetwork - gets a utility network from a geodatabase
    // Direct support for this functionality will eventually be added to the SDK

    public static UtilityNetwork GetUtilityNetworkFromGeodatabase(Geodatabase geodatabase)
    {
      IReadOnlyList<UtilityNetworkDefinition> listUtilityNetworkDefinitions = geodatabase.GetDefinitions<UtilityNetworkDefinition>();
      if (listUtilityNetworkDefinitions.Count == 1)
      {
        UtilityNetworkDefinition utilityNetworkDefinition = listUtilityNetworkDefinitions[0];
        UtilityNetwork utilityNetwork = geodatabase.OpenDataset<UtilityNetwork>(utilityNetworkDefinition.GetName());
        return utilityNetwork;
      }
      return null;
    }


    // GetNetworkElementFromGuidAndTerminalID
    // This routine isn't as simple as it might first appear because network elements are returned from the network index by passing 
    // in a Guid.  If the Guid refers to a feature with terminals, a set of network elements will be returned, only one of which 
    // will map to the TerminalID that is included with the FeatureElement.
    // Note that when this routine is called on an edge with multiple elements, the first edge will be returned (Feature elements
    // do not provide a way to identify a particular edge)

    public static NetworkElement GetNetworkElementFromGuidAndTerminalID(UtilityNetworkTopology utilityNetworkTopology, Guid globalID, int terminalID)
    {
      // Get a list of the network elements that correspond to this guid
      IReadOnlyList<NetworkElement> networkElements = utilityNetworkTopology.GetNetworkElements(globalID);

      // For each network element, convert to a feature element
      foreach (NetworkElement networkElement in networkElements)
      {
        FeatureElement foundFeatureElement = utilityNetworkTopology.GetFeatureElement(networkElement);
        if (foundFeatureElement.Terminal == null)
        {
          // We have an edge.  If we asked for an edge, we've found our network element
          if (terminalID == -1)
          {
            return networkElement;
          }
        }
        else if (foundFeatureElement.Terminal.ID == terminalID)
        {
          return networkElement;
        }
      }
      return null;
    }

		// GetNetworkAttributeByName - returns a NetworkAttribute with the given name
		// This routine might be added to future versions of the SDK
		public static NetworkAttribute GetNetworkAttributeByName(UtilityNetworkDefinition utilityNetworkDefinition, string name)
		{
			return utilityNetworkDefinition.GetNetworkAttributes().First(x => x.Name == name);
		}

		// GetSubtypeByName - returns a subtype with the given name
		// This routine might be added to future versions of the SDK
		public static Subtype GetSubtypeByName(FeatureClassDefinition featureClassDefinition, string name)
		{
			return featureClassDefinition.GetSubtypes().First(x => x.GetName() == name);
		}
  }
}

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
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace LoadReportSample
{
  internal class LoadReportSample : Module
  {
    private static LoadReportSample _this = null;

    /// <summary>
    /// This addin demonstrates the creation of a simple electric distribution report.  It traces downstream from a given point and adds up the count of customers and total load per phase.  This sample
    /// is meant to be a demonstration on how to use the Utility Network portions of the SDK.  The report display is rudimentary.  Look elsewhere in the SDK for better examples on how to display data.
    /// 
    /// Rather than coding special logic to pick a starting point, this sample leverages the existing Set Trace Locations tool that is included with the product.
    /// That tool writes rows to a table called UN_Temp_Starting_Points, which is stored in the default project workspace.  This sample reads rows from that table and uses them as starting points
    /// for our downstream trace.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu.  Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open.
    /// 4. Open a map view that contains at least one Feature Layer whose source points to a Feature Class that participates in a utility network.
    /// 5. Select a feature layer that participates in a utility network
    /// 6. Click on the SDK Samples tab on the Utility Network tab group
    /// 7. Click on the Starting Points tool to create a starting point on the map
    /// 8. Click on the Create Load Report tool
    /// </remarks>
    public static LoadReportSample Current
    {
      get
      {
        return _this ?? (_this = (LoadReportSample)FrameworkApplication.FindModule("LoadReportSample_Module"));
      }
    }

    #region Overrides
    /// <summary>
    /// Called by Framework when ArcGIS Pro is closing
    /// </summary>
    /// <returns>False to prevent Pro from closing, otherwise True</returns>
    protected override bool CanUnload()
    {
      //TODO - add your business logic
      //return false to ~cancel~ Application close
      return true;
    }

    #endregion Overrides

  }
}

## ArcGIS Pro 1.4 Utility Network Alpha SDK

The utility network in ArcGIS provides organizations that manage networks – like electric, telecom, gas, sewer, and water lines – an extensible solution that focuses on performance, scalability, interoperability, and data integrity. 

This repository describes the ArcGIS Pro SDK to use and extend the utility network.  The wiki contains documentation.  Code samples show how to author add-ins for ArcGIS Pro.

The SDK described here will evolve based upon user feedback and the evolution of the platform.

###Table of Contents

* [Samples](#samples)
* [Documentation](#documentation)
* [Requirements](#requirements)
* [Resources](#resources)
* [Known Issues](#known-issues)

-------------------------
##Samples
This repository contains ArcGIS Pro Add-in Samples for the ArcGIS Pro 1.4 SDK for Microsoft .NET Framework.  These samples demonstrate the key functions of the Utility Network.

* [Electric Distribution Load Report (C#)](https://github.com/esri/utility-network-sdk/tree/master/LoadReportSample)  **Note that this sample does not work with Alpha 10.  This functionality will be restored in a subsequent alpha release.**

*To compile ArcGIS Pro SDK samples using ArcGIS Pro 1.4, you must set the Target framework in your Visual Studio project properties to ".NET Framework 4.6.1."*

##Documentation
**New in Alpha 10!**  This repository contains a PowerPoint describing the major classes inside the Utility Network SDK.  While the SDK is still evolving, this document is up-to-date with the SDK that shipped with Alpha 10.

This repository also contains a UtilityNetworkSDK.chm file.  This contains an API reference for this alpha release.  Utility Network classes are contained within the ArcGIS.Core.Internal.Data.UtilityNetwork namespace (post-ship, they will move to ArcGIS.Core.Data.UtilityNetwork).  This API reference is not yet complete, and will evolve over time.  For classes outside of the utility network, we recommend you continue to use the [official documentation](http://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic1.html).

##Requirements
The requirements and supported software needed to develop utility network add-ins.

####Required ArcGIS Software 
* ArcGIS Pro set up for the ArcGIS Pro 1.4 Utility Network Alpha release
* ArcGIS Pro SDK for .Net

####Supported Platforms
* Windows 8.1 Basic, Professional, and Enterprise (64 bit [EM64T]) 
* Windows 8 Basic, Professional, and Enterprise (64 bit [EM64T]) 
* Windows 7 SP1 Ultimate, Enterprise, Professional, and Home Premium (64 bit [EM64T]) 

####Supported .NET framework
* [Microsoft .NET Framework 4.6.1 Developer Pack](https://www.microsoft.com/en-us/download/details.aspx?id=49978)

####Supported IDEs
* Visual Studio 2015 (Professional, Enterprise, and Community Editions)
* Visual Studio 2013 (Professional, Premium, Ultimate, and Community Editions)

##Resources
* [ArcGIS Pro 1.4 Utility Network Alpha 10](https://earlyadopter.esri.com/project/version/item.html?cap=2578B1991F9E43C7B114BD1BB37462C9&arttypeid={13C846C4-9434-4B46-A34C-97D3F4DBCDF3}&artid={8206B490-1BD2-4F83-ACBA-C06BB9BE4168}) 
* [Installing ArcGIS Pro SDK](https://github.com/Esri/arcgis-pro-sdk#installing-arcgis-pro-sdk-for-net)
* [Building add-ins for ArcGIS Pro](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Build-your-first-add-in)
* [ArcGIS Pro 1.3 API Reference Guide](http://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic1.html).  Note that the Pro API Reference Guide has not yet been updated for 1.4.

##Known Issues
* Tracing does not work in the Alpha 10 SDK.

# ConfigureLoadReportData.py
#
# This script should be run against a Naperville sample database
# It configures the database and utility network to have the necessary fields, network attributes, and categories
# to run the Create Load Report SDK sample

import arcpy

# Parameters

utilityNetwork = arcpy.GetParameterAsText(0)
electricDistributionDeviceFeatureClass = arcpy.GetParameterAsText(1)

# Constants

DomainNetwork = "ElectricDistribution"
ServicePointAssetGroup = "ServicePoint"
LoadField = "SERVICEPOINTLOAD"
LoadFieldAlias = "Load"
LoadAttribute = "Load"
ServicePointCategoryName = "ServicePoint"

# Disable the Utility Network topology
arcpy.AddMessage("Disabling Utility Network Topology")
arcpy.un.DisableNetworkTopology(utilityNetwork)

# Add a Load field to the Device table
arcpy.AddMessage("Adding field to device table")
arcpy.management.AddField(electricDistributionDeviceFeatureClass, LoadField, "SHORT", None, None, None, LoadFieldAlias, "NULLABLE", "NON_REQUIRED", None)

# Populate the Load field with some sample values (200mA)
arcpy.AddMessage("Populating field in device table")
arcpy.management.CalculateField(electricDistributionDeviceFeatureClass, LoadField, "SetLoad(!AssetGroup!)", "PYTHON_9.3", r"def SetLoad(AssetGroup):\n  if (AssetGroup == 12):\n    return 200\n  else:\n    return 0")

# Create a Network Attribute to represent Load
arcpy.AddMessage("Creating network attribute")
arcpy.un.AddNetworkAttribute(utilityNetwork, LoadAttribute, "SHORT", False, None, False)

# Assign the Network Attribute to our newly created and populated Load field
arcpy.AddMessage("Assigning network attribute to field")
arcpy.un.SetNetworkAttribute(utilityNetwork, LoadAttribute, DomainNetwork, electricDistributionDeviceFeatureClass, LoadField)

# Add a ServicePoint category
arcpy.AddMessage("Adding Utility Network category")
arcpy.un.AddNetworkCategory(utilityNetwork, ServicePointCategoryName)

# Assign the ServicePoint category to our ServicePoint AssetGroup
arcpy.AddMessage("Assigning Utility Network category")
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, ServicePointAssetGroup, "Meter", ServicePointCategoryName)
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, ServicePointAssetGroup, "Primary Meter", ServicePointCategoryName)

# Re-enable the Utility Network topology
arcpy.AddMessage("Enabling Utility Network Topology")
arcpy.un.EnableNetworkTopology(utilityNetwork)
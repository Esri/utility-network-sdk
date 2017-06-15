# ConfigureLoadReportData.py
#
# This script should be run against a Naperville sample database
# It configures the database and utility network to have the necessary categories
# to run the Create Load Report SDK sample
#
# 1. Before running this script, make sure to shut off all services that use this database.
# 2. Run this script when connected via client-server
# 3. In addition, if you running this from ArcGIS Pro, activate your utility network layer before executing this tool.

import arcpy

# Parameters

utilityNetwork = arcpy.GetParameterAsText(0)
electricDistributionDeviceFeatureClass = arcpy.GetParameterAsText(1)

# Constants

DomainNetwork = "ElectricDistribution"
ServicePointCategoryName = "ServicePoint"
LoadField = "SERVICE_LOAD"
LoadAttribute = "Customer Load"

# Disable the Utility Network topology
arcpy.AddMessage("Disabling Utility Network Topology")
arcpy.un.DisableNetworkTopology(utilityNetwork)

# Create a Network Attribue to represent Load
arcpy.AddMessage("Creating network attribute")
arcpy.un.AddNetworkAttribute(utilityNetwork, LoadAttribute, "SHORT", False, None, False)

# Assign the network attribute to the SERVICE_LOAD field in the ElectricDistributionDevice table
arcpy.AddMessage("Assigning network attribute to field")
arcpy.un.SetNetworkAttribute(utilityNetwork, LoadAttribute, DomainNetwork, electricDistributionDeviceFeatureClass, LoadField )

# Add a ServicePoint category
arcpy.AddMessage("Adding Utility Network category")
arcpy.un.AddNetworkCategory(utilityNetwork, ServicePointCategoryName)

# Assign the ServicePoint category to our ServicePoint AssetGroup
arcpy.AddMessage("Assigning Utility Network category")
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Primary Meter", ServicePointCategoryName)
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Single Phase Low Voltage Meter", ServicePointCategoryName)
arcpy.un.SetNetworkCategory(utilityNetwork, DomainNetwork, electricDistributionDeviceFeatureClass, "Service Point", "Three Phase Low Voltage Meter", ServicePointCategoryName)


# Re-enable the Utility Network topology
arcpy.AddMessage("Enabling Utility Network Topology")
arcpy.un.EnableNetworkTopology(utilityNetwork)
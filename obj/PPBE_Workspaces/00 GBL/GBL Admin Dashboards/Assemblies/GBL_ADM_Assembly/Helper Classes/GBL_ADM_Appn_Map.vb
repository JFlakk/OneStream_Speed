Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports Workspace.GBL.GBL_Assembly

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class GBL_ADM_Appn_Map
		Public ReadOnly ColumnMaps As New List(Of GBL_ColumnMap)

		Public Sub New()
			Dim appn As New GBL_ColumnMap("Appropriation_CD","Appropriation_CD",GetType(String),True,"Member","U1_APPN","U1#",6)
			ColumnMaps.Add(appn)
            Dim treasury_Cd As New GBL_ColumnMap("Treasury_CD","Treasury_CD",GetType(String),True,"NA",String.Empty,String.Empty,4)
			ColumnMaps.Add(treasury_Cd)
            Dim years_avail As New GBL_ColumnMap("Years_of_Availability","Years_of_Availability",GetType(String),True,"Numeric",String.Empty,String.Empty,1)
			ColumnMaps.Add(years_avail)
            Dim dollarType As New GBL_ColumnMap("Dollar_Type","Dollar_Type",GetType(String),True,"U4_Member","U4_DollarType","U4#",100)
			ColumnMaps.Add(dollarType)
            Dim supp_id As New GBL_ColumnMap("Supp_ID","Supp_ID",GetType(String),True,"Numeric",String.Empty,String.Empty,1)
			ColumnMaps.Add(supp_id)
            Dim seventh_char As New GBL_ColumnMap("Seventh_Character","Seventh_Character",GetType(String),True,"NA",String.Empty,String.Empty,1)
			ColumnMaps.Add(seventh_char)
            Dim partialFundCode As New GBL_ColumnMap("Partial_Fund_CD","Partial_Fund_CD",GetType(String),True,"NA",String.Empty,String.Empty,10)
			ColumnMaps.Add(partialFundCode)
		End Sub
	End Class
End Namespace
#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Structure;
#endregion

namespace FindSelectedColumn
{
  [Transaction( TransactionMode.Manual )]
  public class Command : IExternalCommand
  {
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Application app = uiapp.Application;
      Document doc = uidoc.Document;
      Selection sel = uidoc.Selection;

      // Reserve all element ids for following iteration
      List<ElementId> selectedIds = new List<ElementId>();
      foreach( ElementId elemId in sel.GetElementIds() )
      {
        Element elem = doc.GetElement( elemId );
        selectedIds.Add( elem.Id );
      }
      if( selectedIds.Count == 0 )
      {
        message = "Please select a concrete beam or column to create rebar.";
        return Result.Failed;
      }

      // Construct filter to find expected rebar host
      // Structural type filters firstly

      LogicalOrFilter stFilter = new LogicalOrFilter(
        new ElementStructuralTypeFilter( StructuralType.Beam ),
        new ElementStructuralTypeFilter( StructuralType.Column ) );

      // + StructuralMaterial 

      LogicalAndFilter hostFilter = new LogicalAndFilter( stFilter,
        new StructuralMaterialTypeFilter( StructuralMaterialType.Concrete ) );

      // Expected rebar host: it should be family instance
      FilteredElementCollector collector
        = new FilteredElementCollector(
          uidoc.Document, selectedIds );

      FamilyInstance rebarHost = collector
        .OfClass( typeof( FamilyInstance ) )
        .WherePasses( hostFilter )
        .FirstElement() as FamilyInstance;

      if( rebarHost == null )
        TaskDialog.Show( "Null", "Not any Concrete Column Selected" );

      return Result.Succeeded;
    }
  }
}

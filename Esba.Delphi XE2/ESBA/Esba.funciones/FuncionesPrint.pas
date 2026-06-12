unit FuncionesPrint;

interface

uses
  GmTypes, GmPreview;

Procedure Caja(x1,y1,x2,y2:Real;VistaPrevia:TGmPreview);

implementation


Procedure Caja(x1,y1,x2,y2:Real;VistaPrevia:TGmPreview);
Begin
  With VistaPrevia.Canvas Do Begin
       MoveTo(x1,y1,GmCentimeters);
       LineTo(x1,y2,GmCentimeters);
       LineTo(x2,y2,GmCentimeters);
       LineTo(x2,y1,GmCentimeters);
       LineTo(x1,y1,GmCentimeters);
  End;
End;

end.

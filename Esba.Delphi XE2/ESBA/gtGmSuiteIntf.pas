{***************************************************************************}
{                                                                           }
{  Gnostice eDocEngine 		                                              }
{                                                                           }
{  Copyright © 2002-2011 Gnostice Information Technologies Private Limited  }
{  http://www.gnostice.com                                                  }
{                                                                           }
{***************************************************************************}

{------------------------------------}
{          Editor Options            }
{------------------------------------}
{                                    }
{ Tab Stops = 2                      }
{ Use Tab Character = True           }
{                                    }
{------------------------------------}

unit gtGmSuiteIntf;

interface
uses
  Classes, Windows, gtCstDocEng, gtXportIntf, GmPreview, GmTypes;

type

  TgtGmSuiteInterface = class(TgtExportInterface)
  public
    procedure RenderDocument(GmPreview1: TGmPreview);
  published
    property Engine;
  end;

implementation
uses
  GMClasses;

{ TgtGmSuiteInterface }

procedure TgtGmSuiteInterface.RenderDocument(GmPreview1: TGmPreview);
var
  LI : Integer;
begin
  with IgtDocumentEngine(Engine) do
  begin
    case GmPreview1.PaperSize of
     TGmPaperSize.A3: Page.PaperSize := gtCstDocEng.A3;
     TGmPaperSize.A4: Page.PaperSize := gtCstDocEng.A4;
     TGmPaperSize.legal: Page.PaperSize := gtCstDocEng.legal;
     TGmPaperSize.letter: Page.PaperSize := gtCstDocEng.letter;
     else
      Page.PaperSize := gtCstDocEng.Custom;
    end;

    Engine.MeasurementUnit := muMM;// Pixels;

    for LI := 1 to GmPreview1.NumPages do
    begin
      Page.Width :=  GmPreview1.PageWidth.AsMillimeters;
      Page.Height :=  GmPreview1.PageHeight.AsMillimeters;
      if LI = 1 then
        StartDocument
      else
        NewPage;

      GmPreview1.Pages[li].DrawToCanvas(Engine.Canvas, 98);
    end;
    EndDocument;
  end;
end;

end.

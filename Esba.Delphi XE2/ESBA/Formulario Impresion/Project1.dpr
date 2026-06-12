program Project1;

uses
  Forms,
  Imprimir in 'Imprimir.pas' {FormPreview};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFormPreview, FormPreview);
  Application.Run;
end.

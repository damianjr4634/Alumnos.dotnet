program Project1;

uses
  Forms,
  altamodifmaterias in 'altamodifmaterias.pas' {AlMoMaterias},
  Funciones in '..\ESBA\Funciones.pas',
  Imprimir in '..\Formulario Impresion\Imprimir.pas' {FormPreview},
  FrmdeEspera in '\\Esba-2\NEW PAILOT\New Pailot\PRG\Formulario Video de espera\FrmdeEspera.pas' {VideoEspera};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TAlMoMaterias, AlMoMaterias);
  Application.CreateForm(TVideoEspera, VideoEspera);
  Application.Run;
end.

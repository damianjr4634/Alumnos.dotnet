program Project1;

uses
  Forms,
  InscripcionDeMaterias in 'InscripcionDeMaterias.pas' {InscripcionMaterias},
  Funciones in '\\Esba-2\NEW PAILOT\New Pailot\PRG\ESBA\Funciones.pas',
  FrmdeEspera in '\\Esba-2\NEW PAILOT\New Pailot\PRG\Formulario Video de espera\FrmdeEspera.pas' {VideoEspera};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TInscripcionMaterias, InscripcionMaterias);
  Application.Run;
end.

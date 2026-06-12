program Project1;

uses
  Forms,
  ModifAnalitico in 'ModifAnalitico.pas' {AMAnalitico},
  Funciones in '..\ESBA\Funciones.pas',
  FrmdeEspera in '\\Esba-2\NEW PAILOT\New Pailot\PRG\Formulario Video de espera\FrmdeEspera.pas' {VideoEspera};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TAMAnalitico, AMAnalitico);
  Application.Run;
end.

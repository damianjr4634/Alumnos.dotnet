program Project1;

uses
  Forms,
  cargacomisiones in 'cargacomisiones.pas' {FrmComArmadas},
  Funciones in '\\Esba-2\NEW PAILOT\New Pailot\PRG\ESBA\Funciones.pas',
  FrmdeEspera in '\\Esba-2\NEW PAILOT\New Pailot\PRG\Formulario Video de espera\FrmdeEspera.pas' {VideoEspera};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFrmComArmadas, FrmComArmadas);
  Application.Run;
end.

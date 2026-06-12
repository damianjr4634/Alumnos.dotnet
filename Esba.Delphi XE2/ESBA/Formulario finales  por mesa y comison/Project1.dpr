program Project1;

uses
  Forms,
  FinalesxMesayComision in 'FinalesxMesayComision.pas' {Finalesmesacom},
  FrmdeEspera in '\\Esba-2\NEW PAILOT\New Pailot\PRG\Formulario Video de espera\FrmdeEspera.pas' {VideoEspera},
  Funciones in '\\Esba-2\NEW PAILOT\New Pailot\PRG\ESBA\Funciones.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFinalesmesacom, Finalesmesacom);
  Application.CreateForm(TVideoEspera, VideoEspera);
  Application.Run;
end.

program Project1;

uses
  Forms,
  CertServ in 'CertServ.pas' {FrmCertServ},
  FrmdeEspera in '\\Esba-2\NEW PAILOT\New Pailot\PRG\Formulario Video de espera\FrmdeEspera.pas' {VideoEspera},
  Funciones in '\\Esba-2\NEW PAILOT\New Pailot\PRG\ESBA\Funciones.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFrmCertServ, FrmCertServ);
  Application.Run;
end.

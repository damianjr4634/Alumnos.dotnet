program Project1;

uses
  Forms,
  enviocorreo in 'enviocorreo.pas' {Frmenviomail},
  Funciones in '..\esba\funciones.pas',
  FrmdeEspera in '..\Formulario Video de espera\FrmdeEspera.pas' {VideoEspera},
  CnfMail in '..\Formulario de configuracion de correo\CnfMail.pas' {FrmCnfMail};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFrmenviomail, Frmenviomail);
  Application.Run;
end.

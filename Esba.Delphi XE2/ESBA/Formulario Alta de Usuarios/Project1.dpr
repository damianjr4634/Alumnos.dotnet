program Project1;

uses
  Forms,
  AltaUsuario in 'AltaUsuario.pas' {FrmAltaUsuario},
  Funciones in '\\Esba-2\NEW PAILOT\New Pailot\PRG\ESBA\Funciones.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFrmAltaUsuario, FrmAltaUsuario);
  Application.Run;
end.

program Project1;

uses
  Forms,
  BajaUsuarios in 'BajaUsuarios.pas' {FrmBajaUsuarios},
  Funciones in '\\Esba-2\NEW PAILOT\New Pailot\PRG\ESBA\Funciones.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFrmBajaUsuarios, FrmBajaUsuarios);
  Application.Run;
end.

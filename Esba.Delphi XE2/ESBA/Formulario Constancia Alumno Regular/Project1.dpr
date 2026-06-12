program Project1;

uses
  Forms,
  constanciaalumnoregular in 'constanciaalumnoregular.pas' {ConstAlumnoRegular},
  Funciones in '\\Esba-2\NEW PAILOT\New Pailot\PRG\ESBA\Funciones.pas';

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TConstAlumnoRegular, ConstAlumnoRegular);
  Application.Run;
end.

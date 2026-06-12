program Project1;

uses
  Forms,
  constanciaalumnos in 'constanciaalumnos.pas' {ConstanciadelAlumno},
  Funciones in '..\ESBA\Funciones.pas',
  Imprimir in 'g:\New Pailot\PRG\Formulario Impresion\Imprimir.pas' {FormPreview};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TConstanciadelAlumno, ConstanciadelAlumno);
  Application.CreateForm(TFormPreview, FormPreview);
  Application.Run;
end.

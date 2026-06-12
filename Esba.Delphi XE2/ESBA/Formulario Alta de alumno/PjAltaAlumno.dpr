program PjAltaAlumno;

uses
  Forms,
  FrmAltaAlumno in 'FrmAltaAlumno.pas' {AltaAlumno};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TAltaAlumno, AltaAlumno);
  Application.Run;
end.

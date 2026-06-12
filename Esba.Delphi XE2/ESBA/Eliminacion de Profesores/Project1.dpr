program Project1;

uses
  Forms,
  ElimProfes in 'ElimProfes.pas' {EliminacionProfes};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TEliminacionProfes, EliminacionProfes);
  Application.Run;
end.

program Project1;

uses
  Forms,
  MesasExamen in 'MesasExamen.pas' {mesasexamenes};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(Tmesasexamenes, mesasexamenes);
  Application.Run;
end.

program Project1;

uses
  Forms,
  sesion in 'sesion.pas' {iniciodesesion};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(Tiniciodesesion, iniciodesesion);
  Application.Run;
end.

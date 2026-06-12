program Project1;

uses
  Forms,
  NotasExamenFinal in 'NotasExamenFinal.pas' {NotasExamenes};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TNotasExamenes, NotasExamenes);
  Application.Run;
end.

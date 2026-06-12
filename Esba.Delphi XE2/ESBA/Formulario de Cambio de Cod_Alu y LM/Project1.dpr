program Project1;

uses
  Forms,
  CambioCodAlu_LM in 'CambioCodAlu_LM.pas' {ModifCodAlu_LM};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TModifCodAlu_LM, ModifCodAlu_LM);
  Application.Run;
end.

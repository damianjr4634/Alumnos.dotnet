program Project1;

uses
  Forms,
  Equivalencia in 'Equivalencia.pas' {FRMEquivalencias},
  Funciones in '..\ESBA\Funciones.pas',
  CambioCodAlu_LM in 'g:\New Pailot\PRG\Formulario de Cambio de Cod_Alu y LM\CambioCodAlu_LM.pas' {ModifCodAlu_LM};

{$R *.RES}

begin
  Application.Initialize;
  Application.CreateForm(TFRMEquivalencias, FRMEquivalencias);
  Application.Run;
end.

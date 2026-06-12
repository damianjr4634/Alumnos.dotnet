unit FuncionesVariant;

interface

uses
  System.Variants, System.SysUtils;

function IfThenVariant(const cond:boolean; const valtrue, valfalse: variant):variant;
function TextToNull(valor:Variant):Variant;

implementation


function IfThenVariant(const cond:boolean; const valtrue, valfalse: variant):variant;
begin
  if cond then
     result:= valtrue
  else
     result:= valfalse;
end;

Function TextToNull(valor:Variant):Variant;
begin
  result:=null;
  case VarType(Valor) of
    varString, varUString, varStrArg:
        if (VarToStr(Valor)<>'') then
           Result:=Valor;
    varDate:
        if VarToDateTime(valor)<>StrToDate('30/12/1899') then
           Result:=Valor;
    else
        Result:=Valor;
  end;
end;

end.

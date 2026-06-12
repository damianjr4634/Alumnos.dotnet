unit FuncionesCuotas;

interface

uses
  System.SysUtils, System.Variants;

function PrimerVencimCuota(const year, month: word):word;

implementation

uses
  Datamodule, funcionesDB;

function PrimerVencimCuota(const year, month: word):word;
var
  x: integer;
  fecha: TDateTime;
begin
  for x := 5 to 10 do begin
    if not (DayOfWeek(EncodeDate(year,month,x)) in [1,7]) then begin
      if not funcionesDB.SqlValidate('SELECT * FROM TBL_FERIADOS WHERE FECHA='''+FormatDateTime('mm/dd/yyyy',EncodeDate(year,month,x))+'''') then begin
        result:=x;
        break;
      end;
    end;
  end;
end;

end.

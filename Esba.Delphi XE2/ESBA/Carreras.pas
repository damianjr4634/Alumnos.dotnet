unit Carreras;

interface

uses
  DxNavBarCollNs, system.SysUtils;

Type
  TBarraCarreElement=Record
     CodCarre: String;
     Descripcion: string;
     Baja: String;
     EsCarre: String;
     distancia: boolean;
     tipo: string;
     regsolapa: integer;
  end;

var
  BarraCarrerasItems:Array of TBarraCarreElement;
  BarraCarrerasGroup:Array of TdxNavBarGroup;

function CarreIndexOf(const carre:string):TBarraCarreElement;

implementation

function CarreIndexOf(const carre:string):TBarraCarreElement;
var
  x: integer;
begin
  for x := 0 to Length(BarraCarrerasItems)-1 do
    if AnsiSameText(BarraCarrerasItems[x].CodCarre, carre) then begin
      result:= BarraCarrerasItems[x];
      break;
    end;
end;

end.

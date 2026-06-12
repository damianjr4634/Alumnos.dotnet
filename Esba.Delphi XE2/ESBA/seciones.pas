unit seciones;

interface

Uses
  FuncionesSystem, FuncionesDB, FuncionesConfiguracion, SysUtils, Variants;

var
   codsecion:String;

procedure SetCodSecion;
procedure CheckCodSecion;

implementation

Procedure SetCodSecion;
var x:String;
begin
   if (codsecion='') then begin
      FuncionesSystem.GenId(x);
      if (FuncionesDB.ExecScript('UPDATE USUARIOS SET UID='+#39+x+#39+' WHERE CODUSU='+IntToStr(FuncionesConfiguracion.CodUsu))=0) then
         raise Exception.Create('No se pudo grabar el UID de secion')
      else
         codSecion:=x;
   end;
end;

procedure CheckCodSecion;
var b:Variant;
begin
   if codsecion='' then exit;
   //se queda en un cocli sin fin por que la consulta siemrpe llama al star transaction
   FuncionesDB.Consulta('SELECT UID FROM USUARIOS WHERE CODUSU='+IntToStr(FuncionesConfiguracion.CodUsu),b);
   if b[0]<>CodSecion then
      raise Exception.Create('Conexcion bloqueada. Se inicio secion con su usuario en otra maquina');
end;
end.

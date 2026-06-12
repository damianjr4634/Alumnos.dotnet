unit uMensajesError;

interface

uses Classes, SysUtils;

const
  secConstraints = 'Constraints';

procedure CargarDiccionario(const Nombre:string);

function GetConstraintName: string;

function MostrarMensajeIB(E:Exception;SQL:String): Boolean;

implementation

uses IniFiles, IB, Dialogs;

var
  Restricciones: TStrings;

procedure CargarDiccionario(const Nombre:string);
var
  Diccio: TIniFile;
begin
  Diccio:= TIniFile.Create(Nombre);
       Diccio.ReadSectionValues(secConstraints,Restricciones);
  Diccio.Free;
end;

function GetConstraintName: string;
var
  p: PStatusVector;
  i: Integer;
begin
       p:= StatusVectorArray;
  Result:= '';
  i:= 0;
  while p[i]<>0 do
  begin
    if p[i] = 2 then
    begin
           Result:= PChar(p[i+1]);
      Break;
    end
    else
    if p[i]=3 then Inc(i);
    Inc(i,2);
  end; //while
end;

function MostrarMensajeIB(E:Exception; SQL:String): Boolean;
var
  ConstrName,ConstrValue: string;
begin
  if E.ClassNameIs('EIBInterbaseError') then
  begin
    ConstrName:= GetConstraintName;
    ConstrValue:= Restricciones.Values[ConstrName];
    if ConstrValue='' then
           ShowMessage(Format('IBErrorCode: %d - SQLErrorCode: %d',
             [EIBInterbaseError(e).IBErrorCode,EIBInterbaseError(e).SQLCode])+#13+
             'Mensaje: '+e.Message+#13+SQL)
    else
      ShowMessage(ConstrValue);
    Result:= True;
  end
  else
    Result:= False;
end;

initialization
  Restricciones:= TStringList.Create;

finalization
  if Assigned(Restricciones) then Restricciones.Free;

end.

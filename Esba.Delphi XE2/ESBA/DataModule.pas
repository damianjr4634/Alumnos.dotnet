unit DataModule;

interface

uses
  SysUtils, Windows, Messages, Classes, Graphics, Controls, Forms,
  Dialogs,FIBDatabase, pFIBDatabase, FIBQuery, pFIBQuery, FIBDataSet, pFIBDataSet,
  DB;

type
  TCustomerData = class(TDataModule)
    FBase: TpFIBDatabase;
    FtrBase: TpFIBTransaction;
    FTrUsuarios: TpFIBTransaction;
    TrQrVarios: TpFIBTransaction;
    FDsUsuarios: TpFIBDataSet;
    IbQrVarios: TpFIBQuery;
    TrInscMaterias: TpFIBTransaction;
    TrDlVArios: TpFIBTransaction;
    IBDlVArios: TpFIBQuery;
    TrUpVarios: TpFIBTransaction;
    IbupVArios: TpFIBQuery;
    Trconstancia: TpFIBTransaction;
    TrDsVarios: TpFIBTransaction;
    IbDsVarios: TpFIBDataSet;
    TrValidacion: TpFIBTransaction;
    IbValidacion: TpFIBQuery;
    IbCursada: TpFIBQuery;
    TrCursada: TpFIBTransaction;
    IbMaterias: TpFIBQuery;
    TrMaterias: TpFIBTransaction;
    IbAnalitico: TpFIBDataSet;
    TrAnalitico: TpFIBTransaction;
    IBUpdateProf: TpFIBQuery;
    TRUpdateProf: TpFIBTransaction;
    IBProfesores: TpFIBDataSet;
    TrProfesores: TpFIBTransaction;
    TrSertCerv: TpFIBTransaction;
    IbSertCerv: TpFIBDataSet;
    DsAlumnos: TpFIBDataSet;
    TrAlumnos: TpFIBTransaction;
    IbInscmaterias: TpFIBDataSet;
    IbConstancia: TpFIBDataSet;
    procedure DataModuleCreate(Sender: TObject);
    procedure BaseBeforeStart(Sender: TObject);
  private
    { Private declarations }
  public
    function Connect: boolean; virtual;
    function DisConnect: boolean; virtual;
    procedure Excepciones(Sender: TObject; E: Exception);
  end;

resourcestring
  errConnectBD = 'Error conectando con la base de datos.'#13#13'Mesaje de error: '#13'%s';

var
  CustomerData: TCustomerData;
  Servidor,BaseUsuario,BasePass,Base : String;

implementation
 uses
  IBExternals, {$IFDEF PROGRAM_ESBA_ALUMNOS}  seciones, {$ENDIF} uMensajesError;

{$R *.dfm}
{$H+}


procedure TCustomerData.Excepciones(Sender: TObject; E: Exception);
begin
    if not MostrarMensajeIB(E,'') then
       Application.ShowException(E);
end;

function TCustomerData.Connect: boolean;
begin
  Result := true;
  // Si estamos conectados, nos desconectamos
  if FBase.Connected then
    FBase.Connected := false;
  // asignamos nombre de la base de datos
  try
    FBase.Connected := true;
  except
    on E: Exception do
    begin
      ShowMessage(Format(errConnectBD, [E.Message]));
      Result := false;
    end;
  end;
end;

function TCustomerData.DisConnect: boolean;
begin
  Result := true;
  try
    FBase.Connected:=False;
  except
    on E: Exception do
    begin
      ShowMessage(Format(errConnectBD, [E.Message]));
      Result := false;
    end;
  end;
end;

procedure TCustomerData.DataModuleCreate(Sender: TObject);
begin
  Application.OnException:= Excepciones;
end;

procedure TCustomerData.BaseBeforeStart(Sender: TObject);
begin
  {if FBase.Connected then
    DisConnect;

 Connect;}
  if Not FBase.Connected then
     Connect;

 {$IFDEF PROGRAM_ESBA_ALUMNOS}
 if TpFIBTransaction(Sender).Name<>'FtrBase' then
     Seciones.CheckCodSecion;
 {$ENDIF}
end;


end.

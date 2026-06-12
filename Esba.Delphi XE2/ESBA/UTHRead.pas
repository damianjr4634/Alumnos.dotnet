unit UTHRead;

interface

uses
  Windows, Messages, SysUtils, Classes, Controls, Forms,
  FIBDatabase, pFIBDatabase, FIBQuery, pFIBQuery, FIBDataSet, pFIBDataSet, System.Variants,
  pFIBProps, kbmMemTable, VCL.ComCtrls, Data.DB;

type
  TFISQLThread = class(TThread)
  private
    FIBDataSet: TpFIBDataSet;
    FIBTransaction: TpFIBTransaction;
    FIBConnection: TpFIBDatabase;
    FSQL: string;      // SQL To execute
    FID: integer;      // Internal ID

  public
    AKeyUpdate: String;
    AFieldKey: String;

    AFinish: Boolean;
    AMemTable: TKbmMemTable;
    AProgressBar: TProgressBar;
    SDataBase: String;
    SUser: String;
    SPassWord: String;
    SCharSet: String;
    ASql: String;
    TPSynchro: TProc;
    AErrorMessage: String;

    constructor Create(const CreateSuspended:Boolean; const IDThread:integer);
    destructor Destroy; override;
    procedure Execute; override;

    property ID:integer read FID write FID;
    property SQL:string read FSQL write FSQL;
    property AIBDataSet:TpFIBDataSet read FIBDataSet write FIBDataSet;
    property AIBTransaction: TpFIBTransaction read FIBTransaction write FIBTransaction;
    property AIBConnection: TpFIBDatabase read FIBConnection write FIBConnection;
  end;

implementation


constructor TFISQLThread.Create(const CreateSuspended: Boolean; const IDThread: Integer);
begin
  inherited Create(CreateSuspended);

  self.FreeOnTerminate:=false;

  FIBDataSet:= TpFIBDataSet.Create(nil);
  FIBTransaction := TpFIBTransaction.Create(nil);
  FIBConnection:= TpFIBDatabase.Create(nil);

  FIBDataSet.Transaction:= FIBTransaction;
  FIBDataSet.Database:= FIBConnection;
  FIBTransaction.DefaultDatabase:= FIBConnection;

  FID:=IDThread;
end;

destructor TFISQLThread.Destroy;
begin
  try
    FIBDataSet.Close;
    if FIBTransaction.Active then
      FIBTransaction.Rollback;
    FIBConnection.Connected:=False;
  finally
    FIBDataSet.Transaction:= nil;
    FIBDataSet.Database:= nil;
    FIBTransaction.DefaultDatabase:= nil;

    FreeAndNil(FIBDataSet);
    FreeAndNil(FIBTransaction);
    FreeAndNil(FIBConnection);
  end;

  inherited;
end;

procedure TFISQLThread.Execute;
var
  x:integer;
begin
  inherited;
  AErrorMessage:='';
  try
    AFinish:=False;
    FIBConnection.DatabaseName:= SDataBase;
    FIBConnection.ConnectParams.UserName := SUser;
    FIBConnection.ConnectParams.Password := SPassWord;
    FIBConnection.ConnectParams.CharSet := SCharSet;
    FIBConnection.SQLDialect:=3;
    FIBConnection.LibraryName:='fbclient.dll';

    FIBDataSet.SQLs.SelectSQL.Text:=ASql;

    FIBConnection.Connected:=true;
    FIBTransaction.Active:=true;
    FIBDataSet.Open;
    if AKeyUpdate='' then
      AMemTable.Active:=False;

    if Assigned(AMemTable) then begin //tengo memtable
      if (AMemTable.FieldCount=0) then begin //creocampos
        for x := 0 to FIBDataSet.FieldCount-1 do Begin
          AMemTable.FieldDefs.Add(FIBDataSet.Fields[x].Name,FIBDataSet.Fields[x].DataType ,FIBDataSet.Fields[x].Size,False);
        end;
        AMemTable.CreateTable;
      end;
    end;

    FIBDataSet.FetchAll;
    FIBDataSet.First;
    if Assigned(AProgressBar) and (FIBDataSet.RecordCount>0) then
         TThread.Synchronize (TThread.CurrentThread,procedure
                                                    begin
                                                      AProgressBar.Min:=0;
                                                      AProgressBar.Max:= FIBDataSet.RecordCount;
                                                    end);
    AMemTable.Active:=True;
    if AKeyUpdate='' then begin
      while not FIBDataSet.Eof do begin
        AMemTable.Append;
        for x := 0 to AMemTable.FieldCount-1 do
          if not FIBDataSet.FieldByName(AMemTable.Fields.fields[x].fullname).IsNull then
             AMemTable.Fields.fields[x].Value:=FIBDataSet.FieldByName(AMemTable.Fields.fields[x].fullname).Value;

        AMemTable.Post;
        FIBDataSet.Next;

        if Assigned(AProgressBar) then
           TThread.Synchronize (TThread.CurrentThread,procedure
                                                      begin
                                                        AProgressBar.Position:=FIBDataSet.RecNo;
                                                      end);
      end;
    end
    else begin
        if AMemTable.Locate(AFieldKey,AKeyUpdate,[loPartialKey]) then
          AMemTable.Edit
        else
          AMemTable.Append;
        for x := 0 to AMemTable.FieldCount-1 do
          if not FIBDataSet.FieldByName(AMemTable.Fields.fields[x].fullname).IsNull then
            AMemTable.Fields.fields[x].Value:=FIBDataSet.FieldByName(AMemTable.Fields.fields[x].fullname).Value;

        AMemTable.Post;
    end;
    FIBDataSet.Close;
    FIBTransaction.Active:=False;
    FIBDataSet.Active:=False;
    FIBConnection.Connected:=False;
    AFinish:=True;
  except
    on e:Exception do begin
      FIBDataSet.Close;
      FIBTransaction.Active:=False;
      FIBDataSet.Active:=False;
      FIBConnection.Connected:=False;
      AFinish:=True;
      AErrorMessage:=E.Message;
    end;
  end;
end;

end.

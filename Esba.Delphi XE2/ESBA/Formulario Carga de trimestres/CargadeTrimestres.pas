unit CargadeTrimestres;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, DataModule, FIBDataSet, pFIBDataSet, FIBDatabase, pFIBDatabase,
  FIBQuery, pFIBQuery, DB, StdCtrls, Buttons, kbmMemTable, Grids, DBGrids;

type
  TFrmCargaTri = class(TForm)
    DataSource1: TDataSource;
    grilla: TDBGrid;
    MTTrimestre: TkbmMemTable;
    Grabamesa: TBitBtn;
    CancelaGrabacion: TBitBtn;
    MTTrimestreFANIO: TIntegerField;
    MTTrimestreFDDEPRI: TDateField;
    MTTrimestreFHTAPRI: TDateField;
    MTTrimestreFDDESEG: TDateField;
    MTTrimestreFHTASEG: TDateField;
    MTTrimestreFDDETER: TDateField;
    MTTrimestreFHTATER: TDateField;
    IbupIns: TpFIBQuery;
    qrTri: TpFIBQuery;
    Trtri: TpFIBTransaction;
    Dstri: TpFIBDataSet;
    procedure FormActivate(Sender: TObject);
    procedure CancelaGrabacionClick(Sender: TObject);
    procedure GrabamesaClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  FrmCargaTri: TFrmCargaTri;
  modo : String;
implementation

{$R *.dfm}

procedure TFrmCargaTri.FormActivate(Sender: TObject);
var
   x:integer;
begin
  if (modo='TRIMESTRAL') then
     DsTri.SQLs.SelectSQL.Text:='SELECT FANIO, FDDEPRI, FHTAPRI, FDDESEG, FHTASEG, FDDETER, FHTATER FROM TBL_TRIM'
  else
     DsTri.SQLs.SelectSQL.Text:='SELECT FANIO, FDDEPRI, FHTAPRI, FDDESEG, FHTASEG FROM TBL_CUAT';
  if (modo='CUATRIMESTRAL') then begin
    grilla.Columns.Items[5].Visible:=False;
    grilla.Columns.Items[6].Visible:=False;
    MtTrimestre.Fields.FieldByName('FDDETER').Required:=False;
    MtTrimestre.Fields.FieldByName('FHTATER').Required:=False;
  end
  else begin
    grilla.Columns.Items[5].Visible:=True;
    grilla.Columns.Items[6].Visible:=True;
    MtTrimestre.Fields.FieldByName('FDDETER').Required:=True;
    MtTrimestre.Fields.FieldByName('FHTATER').Required:=True;
  end;
  if trtri.Active then
     Trtri.Rollback;
  trtri.Active:=true;
  Dstri.Active:=true;
  dstri.First;
  mttrimestre.Active:=True;
  while not (dstri.Eof) do begin
      Mttrimestre.Append;
      For x:=0 to DsTri.fieldcount-1 do
          MtTrimestre.FieldByName(DsTri.Fields.Fields[x].FullName).Value:=dstri.FieldByName(DsTri.Fields.Fields[x].FullName).Value;
      Mttrimestre.Post;
      dstri.Next;
  end;

  Trtri.Rollback;
  trtri.Active:=False;
  Dstri.Active:=False;
  Mttrimestre.First;
  grilla.Repaint;
end;

procedure TFrmCargaTri.FormCreate(Sender: TObject);
begin
  TrTri.BeforeStart:=CustomerData.BaseBeforeStart;
  if (modo='TRIMESTRAL') then
     FrmCargaTri.Caption:='Carga de trimestres uso 333'
  else
     FrmCargaTri.Caption:='Carga de Cuatrimestres (Para carreras Cuatrimestrales)';
end;

procedure TFrmCargaTri.CancelaGrabacionClick(Sender: TObject);
begin
  Close;
end;

procedure TFrmCargaTri.GrabamesaClick(Sender: TObject);
Var
   x:integer;
begin
  if Mttrimestre.State In [DsInsert,DsEdit] then
     MtTrimestre.Post;
  MtTrimestre.First;
  if trtri.Active then
      TrTri.Rollback;
  trTri.Active:=false;
  if (modo='CUATRIMESTRAL') then
     QrTri.SQL.Text:=' DELETE FROM TBL_CUAT'
  else
     QrTri.SQL.Text:=' DELETE FROM TBL_TRIM';

  trTri.Active:=True;
  QrTri.ExecQuery;
  TRTri.Commit;

  trtRI.Active:=false;
  if (modo='CUATRIMESTRAL') then
     QrTri.SQL.Text:='INSERT INTO TBL_CUAT (FANIO, FDDEPRI, FHTAPRI, FDDESEG, FHTASEG) '+
                     ' VALUES (:FANIO, :FDDEPRI, :FHTAPRI, :FDDESEG, :FHTASEG)'
  else
     QrTri.SQL.Text:='INSERT INTO TBL_TRIM (FANIO, FDDEPRI, FHTAPRI, FDDESEG, FHTASEG, FDDETER, FHTATER) '+
                     ' VALUES (:FANIO, :FDDEPRI, :FHTAPRI, :FDDESEG, :FHTASEG, :FDDETER, :FHTATER)';
  trtri.Active:=True;
  while not mttrimestre.Eof do begin
     For x:=0 to qrtri.ParamCount-1 do
         qrtri.ParamByName(qrTri.ParamName(x)).Value:=MtTrimestre.FieldByName(qrTri.ParamName(x)).Value;
     QrTri.ExecQuery;
     mtTrimestre.Next;
  end;
  trtri.Commit;
  trtri.Active:=false;
  MessageDlg('Registro grabado',mtInformation, [mbok],0,mbok);
  close;
end;

end.

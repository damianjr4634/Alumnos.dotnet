unit lstNotasyPractico;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, Imprimir, GmTypes, GmPreview,gmprinter, FuncionesConfiguracion,
  FuncionesText, FuncionesDB, Printers,
  ComCtrls, DBCtrls, DataModule, Mask, RXToolEdit, FIBDataSet, pFIBDataSet, FIBDatabase, pFIBDatabase,
  DB, StrUtils, Variants, OleServer, ExcelXP, Vcl.FileCtrl;

type
  Posiciones=record
    inicio_enca:real;
    inicio_detalle:real;
    espaciado:real;
    corte:real;
    pos_orden:real;
    pos_nombre:real;
  end;
  TFrmPractNotas = class(TForm)
    GroupBox1: TGroupBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    comision: TEdit;
    cuatrimestre: TEdit;
    Imprimir: TBitBtn;
    salir: TBitBtn;
    BtnExcel: TBitBtn;
    PrintDialog1: TPrintDialog;
    PrintSetup: TPrinterSetupDialog;
    ComboMateria: TComboEdit;
    NombreMat: TLabel;
    IbTr: TpFIBTransaction;
    SqlDatos: TpFIBDataSet;
    SqlComi: TpFIBDataSet;
    procedure comisionKeyPress(Sender: TObject; var Key: Char);
    procedure cuatrimestreKeyPress(Sender: TObject; var Key: Char);
    procedure CodigoMateriaKeyPress(Sender: TObject; var Key: Char);
    procedure ImprimirClick(Sender: TObject);
    procedure salirClick(Sender: TObject);
    procedure ComboMateriaButtonClick(Sender: TObject);
    procedure ComboMateriaExit(Sender: TObject);
    procedure BtnExcelClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  posi:posiciones;
  FrmPractNotas: TFrmPractNotas;
  Plantilla:String;
  VCarrera : String; //sirve para saber la carrera activa
  VUnidad : String; //sirve para saber la unidad en la cual se corre el sistema
  VInstituto  : String; //Pasa el nomnbre del instituto
  VCaracteristica : String; //Pasa la caracteristica del colegio
  VCuatrimestreAnio : String; //guarda el año cuatrimestre actual
  VNombreUsuario : String; //Trae el usuario que esta trabajando
  VCarreraLarga : String; //Trae el nombre completo de la carrera

implementation


{$R *.dfm}

procedure TFrmPractNotas.comisionKeyPress(Sender: TObject; var Key: Char);
begin
  if key=#13 then
  Cuatrimestre.SetFocus;
end;

procedure TFrmPractNotas.cuatrimestreKeyPress(Sender: TObject;
  var Key: Char);
begin
 if key=#13 then
    ComboMateria.SetFocus
end;


procedure TFrmPractNotas.BtnExcelClick(Sender: TObject);
Var
  FrmProgress: TForm;
  Barra : TProgressBar;
  AppExcel: TExcelApplication;
  Libro : _WORKBOOK;
  Hoja  : _WORKSHEET;
  x, ContHojas, contalumnos : Integer;
  Fdir:String;
begin
 if not SelectDirectory('Seleccione Directorio para guardar los archivos', ExtractFileDrive(FDir), FDir,
             [sdNewUI, sdNewFolder]) then
    exit;
 If (ibtr.Active) then begin
    ibtr.Rollback;
    sqlcomi.Active:=false;
    sqldatos.Active:=False;
 end;

 SqlComi.SQLs.SelectSQL.Clear;
 If (ComboMateria.Text = '') and (Comision.Text<>'') then
  Begin
     SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUTUCO = '+Comision.Text+'  AND  C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39+' ORDER BY 3,4';
  end;
 If (ComboMateria.Text = '') and (Comision.Text='') then
   SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39+' ORDER BY 3,4';
 If (ComboMateria.Text <> '') and (Comision.Text<>'') then
   Begin
     SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUTUCO = '+Comision.Text+'  AND  C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39' AND '+
                        'C.COD_MAT = '+#39+Combomateria.Text+#39+' ORDER BY 3,4';;
   End;
  If (ComboMateria.Text <> '') and (Comision.Text='') then
   Begin
     SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39' AND C.COD_MAT = '+#39+Combomateria.Text+#39+'  ORDER BY 3,4';
   End;
 SqlComi.Active := True;
 SqlComi.FetchAll;
 IbTr.Active:=True;
 If SqlComi.RecordCount = 0 Then
   Begin
    MessageDlg(Pchar(VnombreUsuario+', no hay datos para mostrar'),mtError,[mbok],0,mbok);
    Comision.SetFocus;
    SqlComi.Active := False;
    IbTr.RollBack;
    IbTr.Active:=False;
   End
 Else begin
    FrmProgress := TForm.Create(Application);
     Try
       FrmProgress.Caption := 'Exportando.....0%';
       FrmProgress.Position := poDesktopCenter;
       FrmProgress.Width := 400;
       FrmProgress.Height := 60;
       FrmProgress.BorderStyle := bsDialog;
       FrmProgress.Show;
       Barra := TProgressBar.Create(FrmProgress);
       Barra.Parent := FrmProgress;
       Barra.Visible := True;
       Barra.Align := AlClient;
       Barra.Max := SqlComi.RecordCount;
       Barra.Min := 0;
       Barra.Step := 1;
       Barra.Smooth := True;
     Finally

     End;


    While not SqlComi.eof do begin
      Try
        AppExcel:= TExcelApplication.Create(Nil);
      Except
        ShowMessage('Excel no se pudo iniciar.');
        Exit;
      End;
      Libro:=AppExcel.Workbooks.Open(vUnidad+CARPETA_PLANTILLA+'\Planilla_de_notas.xls',EmptyParam,true,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,EmptyParam,0);

      SqlDatos.SQLs.SelectSQL.Text:='SELECT DISTINCT A.COD_ALU, TRIM(A.APELLIDO) AS APELLIDO, TRIM(A.NOM_APE) AS NOM_APE, TRIM(C.CONDICION) AS CONDICION, C.CUTUCO, A.COD_ALU '+
                                       'FROM  CURSADA C '+
                                       'LEFT OUTER JOIN ALUMNOS A  ON C.COD_ALU = A.COD_ALU AND C.CARRE=A.CARRE '+
                                       'WHERE C.CUTUCO = '+IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger)+' AND C.CUA_ANIO = '+#39+Cuatrimestre.Text+#39+
                                       '      AND TRIM(C.CONDICION) IN  ('+#39+'CURSANDO'+#39+','+#39+'RECURSANDO'+#39+')  AND C.COD_MAT = '+#39+SqlComi.FieldByName('COD_MAT').AsString+#39+' AND C.CARRE='+#39+vCARRERA +#39+' AND A.BAJA='+#39+'N'+#39+
                                       'ORDER BY C.CUTUCO, A.APELLIDO, A.NOM_APE ';
       SqlDatos.Active:=True;
       SqlDatos.FetchAll;
       ContHojas:=1;
       contalumnos:=0;
       while not sqlDatos.Eof do begin
          if (ContAlumnos=0) or (ContALumnos>24) then begin
            Hoja:=Libro.WorkSheets.Get_Item(ContHojas) as _WorkSheet;

            Hoja.Cells.Item[4,'A']:= VCarreraLarga;
            Hoja.Cells.Item[5,'A']:= 'Asignatura: '+SqlComi.FieldByName('DESCRIPCI').AsString;
            Hoja.Cells.Item[6,'A']:= 'Cuat.: '+(Copy(IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger),1,1))+'    Turno: '+Turnos(StrtoInt(Copy(IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger),2,1)))+'     División: '+Division(StrtoInt(Copy(IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger),3,1)));
            Hoja.Cells.Item[7,'A']:= 'Con asistencia del Sr/a.Profesor/a: '+(SqlComi.FieldByName('DOCENTE').AsString);
            Hoja.Cells.Item[8,'A']:= 'se procedió a cumplir con el resultado que se consigna a continuación';

            X:=13;
            contAlumnos:=1;
            ContHojas:=conthojas+1;
          end;
          Hoja.Cells.Item[x,'C']:=sqlDatos.FieldByName('APELLIDO').AsString+','+sqlDatos.FieldByName('NOM_APE').AsString;
          Hoja.Cells.Item[x,'B']:=sqlDatos.FieldByName('COD_ALU').AsString;
          X:=X+1;
          SqlDatos.Next;
          contAlumnos:=contalumnos+1;
       end;
       SqlDatos.Close;
       Barra.StepIt;
       FrmProgress.Caption := 'Exportando.....'+FloatToStr(Int((SqlComi.RecNo/Barra.Max)*100))+'%';
       FrmProgress.Refresh;
       deletefile(FDir+'\libro'+IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger)+'_'+SqlComi.FieldByName('DESCRIPCI').AsString+'.xls');
       AppExcel.ActiveWorkbook.Close(true,FDir+'\Notas_'+IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger)+'_'+SqlComi.FieldByName('DESCRIPCI').AsString+'.xls',EmptyParam,0);
       AppExcel.Visible[0] := False;
       AppExcel.Disconnect;
       AppExcel.Free;
       SqlComi.Next;
    end;
  end;
  SqlDatos.Active:=False;
  SqlComi.Active:=False;
  If IbTr.Active then
      IbTr.RollBack;
  IbTr.Active:=False;
end;

procedure TFrmPractNotas.CodigoMateriaKeyPress(Sender: TObject;
  var Key: Char);
begin
  if key=#13 then
    Imprimir.SetFocus;
end;


procedure TFrmPractNotas.ImprimirClick(Sender: TObject);
var
  largopagina:real;
  contareg:integer;
  sw : integer;
  Cutuco_Aux : Integer;
  CodigoMateria_Aux : Integer;
  verti:real;
  x:integer;
  Imagen : TMetaFile;
begin
 sw := 0;
 contareg:=1;
 SqlComi.SQLs.SelectSQL.Clear;
 If (ComboMateria.Text = '') and (Comision.Text<>'') then
  Begin
     SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUTUCO = '+Comision.Text+'  AND  C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39+' ORDER BY 3,4';
  end;
 If (ComboMateria.Text = '') and (Comision.Text='') then
   SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39+' ORDER BY 3,4';
 If (ComboMateria.Text <> '') and (Comision.Text<>'') then
   Begin
     SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUTUCO = '+Comision.Text+'  AND  C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39' AND '+
                        'C.COD_MAT = '+#39+Combomateria.Text+#39+' ORDER BY 3,4';;
   End;
  If (ComboMateria.Text <> '') and (Comision.Text='') then
   Begin
     SqlComi.SQLs.SelectSQL.Text:= 'SELECT TRIM(D.DOCENTE) AS DOCENTE, TRIM(M.DESCRIPCI) AS DESCRIPCI, C.CUTUCO, C.COD_MAT '+
                        'FROM COMARM C '+
                        'LEFT OUTER JOIN DOCENTES D  ON C.CODPROFES = D.CODPROFES '+
                        'LEFT OUTER JOIN MATERIAS M  ON C.COD_MAT = M.CODMATERI AND C.CARRE = M.CODCARRE '+
                        'WHERE C.CARRE = '+#39+VCarrera+#39+' AND C.CUA_ANIO = '+#39+AnsiReplaceText(Cuatrimestre.Text,'/','')+#39' AND C.COD_MAT = '+#39+Combomateria.Text+#39+'  ORDER BY 3,4';
   End;
 SqlComi.Active := True;
 SqlComi.FetchAll;
 IbTr.Active:=True;
 If SqlComi.RecordCount = 0 Then
   Begin
    MessageDlg(Pchar(VnombreUsuario+', no hay datos para mostrar'),mtError,[mbok],0,mbok);
    Comision.SetFocus;
    SqlComi.Active := False;
    IbTr.RollBack;
    IbTr.Active:=False;
   End
 Else
  If PrintDialog1.execute then
   Begin
    FormPreview:=tFormPreview.create(self);
    Imagen := TMetaFile.Create;
    Imagen.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\'+plantilla);
    FormPreview.VistaPrevia1.PaperSize:=Legal;
    FormPreview.VistaPrevia1.Canvas.StretchDraw(0,0,21.54,35.56,Imagen,GmCentimeters);
    largopagina:= posi.inicio_enca;
    While not SqlComi.eof do
     begin
      if not sqldatos.active then
        begin
         SqlDatos.ParamByName('COMI').AsInteger := SqlComi.FieldByName('CUTUCO').AsInteger;
         SqlDatos.ParamByName('CUAT').AsString := Cuatrimestre.Text;
         SqlDatos.ParamByName('MATERIA').AsString := SqlComi.FieldByName('COD_MAT').AsString;
         SqlDatos.ParamByName('CARRERA').AsString := VCarrera;
         SqlDatos.Active:=True;
         SqlDatos.FetchAll;
        end;
      FormPreview.VistaPrevia1.BeginUpdate;
      // **** INICIO DEL ENCABEZADO ***//
      FormPreview.VistaPrevia1.Canvas.Font.Name := 'Arial';
      FormPreview.VistaPrevia1.Canvas.Font.Size := 10;
      FormPreview.VistaPrevia1.Canvas.Font.Style := [FsBold];
      FormPreview.VistaPrevia1.Canvas.TextOutCenter(10.5,LargoPagina,' Planilla de Calificacion de Profesores EMISION: '+DatetoStr(date),gmcentimeters);
      LargoPagina:=LargoPagina+0.5;
      FormPreview.VistaPrevia1.Canvas.TextOutCenter(10.5,LargoPagina,VCarreraLarga,GmCentimeters);
      LargoPagina:=LargoPagina+0.5;
      FormPreview.VistaPrevia1.Canvas.TextOutCenter(10.5,LargoPagina,'Asignatura: '+SqlComi.FieldByName('DESCRIPCI').AsString,GmCentimeters);
      LargoPagina:=LargoPagina+0.5;
      FormPreview.VistaPrevia1.Canvas.TextOutCenter(10.5,LargoPagina,'Cuat.: '+(Copy(IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger),1,1))+'    Turno: '+Turnos(StrtoInt(Copy(IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger),2,1)))+'     División: '+Division(StrtoInt(Copy(IntToStr(SqlComi.FieldByName('CUTUCO').AsInteger),3,1))),GmCentimeters);
      LargoPagina:=LargoPagina+0.5;
      FormPreview.VistaPrevia1.Canvas.TextOut(1,LargoPagina,'Profesor: '+(SqlComi.FieldByName('DOCENTE').AsString)+'    Ciclo Lectivo: '+Copy(DatetoStr(Date),7,4),GmCentimeters);
      LargoPagina:=LargoPagina+1;
      // **** INICIO DEL DETALLE ***//
      FormPreview.VistaPrevia1.Canvas.Font.Name := 'Arial';
      FormPreview.VistaPrevia1.Canvas.Font.Size := 8;
      FormPreview.VistaPrevia1.Canvas.Font.Style := [];
      LargoPagina := posi.inicio_detalle;
      //llenar los parametro de sqlcursada y ejecutarlo
      While (not SqlDatos.eof) and  (largopagina<=posi.corte)  do
       begin
           If Contareg < 10 then
              FormPreview.VistaPrevia1.Canvas.TextOut(posi.pos_orden+0.1,largopagina,IntToStr(Contareg),gmcentimeters)
           else
              FormPreview.VistaPrevia1.Canvas.TextOut(posi.pos_orden,largopagina,IntToStr(Contareg),gmcentimeters);
           contareg:=contareg+1;
           //TOMA DATOS DE LA BD
           FormPreview.VistaPrevia1.Canvas.Font.Name := 'Arial';
           FormPreview.VistaPrevia1.Canvas.Font.Size := 8;
           FormPreview.VistaPrevia1.Canvas.Font.Style := [];
           FormPreview.VistaPrevia1.Canvas.TextOut(posi.pos_nombre,largopagina,SqlDatos.FieldByName('APELLIDO').AsString + ', ' + SqlDatos.FieldByName('NOM_APE').AsString ,gmcentimeters);
           LargoPagina := LargoPagina + posi.espaciado;
           SqlDatos.next;
           if (SqlDatos.FieldByName('CONDICION').AsString = 'RECURSANDO') and (sw=0) then
              begin
                 sw := 1;
                 //FORMATO DEL ENCABEZADO DE RECURSANTES
                 FormPreview.VistaPrevia1.Canvas.Font.Name := 'Arial';
                 FormPreview.VistaPrevia1.Canvas.Font.Size := 10;
                 FormPreview.VistaPrevia1.Canvas.Font.Style := [FsBold];
                 FormPreview.VistaPrevia1.Canvas.TextOut(posi.pos_nombre,largopagina-0.1,'RECURSANTES',GmCentimeters);
                 FormPreview.VistaPrevia1.Canvas.Font.Name := 'Arial';
                 FormPreview.VistaPrevia1.Canvas.Font.Size := 8;
                 FormPreview.VistaPrevia1.Canvas.Font.Style := [];
                 // FIN FORMATO ENCABEZADO DE RECURSANTES
                 LargoPagina := LargoPagina + posi.espaciado;
                 contareg:=1;
               end;
       end;
        FormPreview.VistaPrevia1.EndUpdate;
        if SqlDatos.eof then
         Begin
           SqlDatos.Active:=False;
           SqlComi.Next;
           sw:=0;
           contareg:=1;
         end;
        if (not sqlcomi.Eof) or (largopagina>posi.corte) then begin
          FormPreview.VistaPrevia1.Newpage;
          FormPreview.VistaPrevia1.Canvas.StretchDraw(0,0,21.54,35.56,Imagen,GmCentimeters);
        end;
        LargoPagina := posi.inicio_enca;
     end; //end del while sqlcomi
     FormPreview.VistaPrevia1.CurrentPage := 1;
     FormPreview.ShowModal;
     FormPreview.Free;
     Comision.SetFocus;
  end;
  SqlDatos.Active:=False;
  SqlComi.Active:=False;
  If IbTr.Active then
      IbTr.RollBack;
  IbTr.Active:=False;

end;

procedure TFrmPractNotas.salirClick(Sender: TObject);
begin
  close;
end;

procedure TFrmPractNotas.ComboMateriaButtonClick(Sender: TObject);
  Var b:Variant;
begin
  FuncionesDB.combobusqueda('SELECT CODMATERI, DESCRIPCI FROM MATERIAS WHERE CODCARRE='+#39+VCARRERA+#39+' AND ESTADO<>'+#39+'B'+#39,['Codigo','Nombre'],b,True);
  ComboMateria.Text:=b[0];
  NombreMat.Caption:=b[1];
end;

procedure TFrmPractNotas.ComboMateriaExit(Sender: TObject);
Var b:Variant;
begin
  if (ComboMateria.Text<>'') Then Begin
      ComBoMateria.Text:=Lpad(ComboMateria.Text,2,'0');
      FuncionesDB.combobusqueda('SELECT CODMATERI, DESCRIPCI FROM MATERIAS WHERE CODCARRE='+#39+VCARRERA+#39+' AND CODMATERI='+#39+ComBoMateria.Text+#39+' AND ESTADO<>'+#39+'B'+#39,['Codigo','Nombre'],b,False);
      If (vartostr(b[0])='') then begin
         MessageDlg('Código incorrecto',mtError,[mbok],0,mbok);
         ComboMateria.SetFocus;
      end
      else begin
          ComboMateria.Text:=b[0];
          NombreMat.Caption:=b[1];
      end
  end
end;

end.

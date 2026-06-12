unit Impresiones;

interface

Uses
  SysUtils, Printers, Windows, GmTypes, GmPreview, Messages, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, Db, Grids, DBGrids, OleServer, ExcelXP, ComCtrls,
  busqueda,FIBDatabase, pFIBDatabase, FIBQuery,
  pFIBQuery, FIBDataSet, pFIBDataSet, datamodule,
  Imprimir, gmprinter, FuncionesConfiguracion, FuncionesText,kbmMemTable, Variants,
  StrUtils;

Type
   TClassImpresiones = Class
      PrintDialog: TPrintDialog;
      PrintSetup: TPrinterSetupDialog;
      IbTr: TpFIBTransaction;
      SqlDatos: TpFIBDataSet;
      SqlDatos2: TpFIBDataSet;
      SqlDatos3: TpFIBDataSet;
      SqlDatos4: TpFIBDataSet;
      SqlDatos5: TpFIBDataSet;
      SqlDatos6: TpFIBDataSet;
      Imagen : TMetaFile;
      Imagen2 : TMetaFile;
      PlantillaJpg : TPicture;
      ImageJPG : TPicture;
      largopagina:real;
      margen:real;
   private

   public
      Constructor Inicializa;
      Destructor Finaliza;
      procedure SetFont(const font: String; const size: integer; const Style: TFontStyles);
   end;

function Imp_Mesas_citacion_enc(ClassImpresiones:TclassImpresiones; copia:string; fila:real):real;
function Imp_Mesas_citacion_pie(ClassImpresiones:TclassImpresiones; fila:real; Firma, Firmaimg:String):real;
procedure Imp_Mesas_citacion(CONST docdde, dochta, fecdde, fechta, Carre, vunidad, firma, Firmaimg :string);
procedure Imp_Mesas_ParteDiario(CONST fecdde, fechta, fcarre, vunidad :string);
Procedure Imp_Recincorp_todos(Const MtFaltas:TKbmMemtable; Vunidad, Carrera, Vinstituto,codalu:String);
Procedure Imp_Boletin_calif(carre, codalu, Vunidad, baja :String; anio, cutuco, Sanciones:Integer);
Procedure Imp_Boletin_calif_formatonuevo(carre, codalu, Vunidad, baja :String; anio, cutuco, Sanciones:Integer);
Procedure Imp_Boletin_Inasistencias(carre, codalu, Vunidad, baja :String; anio, cutuco :Integer);
Procedure Imp_Calificador(const carre, codalu, Vunidad, baja:String;const anio, cutuco: integer; const estado: string);
Procedure Imp_EtiqLegajo(const carre, codalu, Vunidad, baja:String;const anio, cutuco: integer);

Var
   ClassImpresiones:TClassImpresiones;
   unidadPrograma: string;

implementation

Destructor TClassImpresiones.Finaliza;
begin
     printdialog.Free;
     if ibtr.Active then
        ibtr.Rollback;
     sqldatos.Close;
     sqldatos2.Close;
     sqldatos3.Close;
     sqldatos4.Close;
     sqldatos5.Close;
     sqldatos6.Close;

     ibtr.Free;
     sqldatos.Free;
     sqldatos2.Free;
     sqldatos3.Free;
     sqldatos4.Free;
     sqldatos5.Free;
     sqldatos6.Free;
     FormPreview.Free;
     Imagen.Free;
     Imagen2.Free;
     ImageJPG.Free;
     PlantillaJpg.Free;
end;


Constructor TClassImpresiones.inicializa;
begin
     printdialog:=Tprintdialog.Create(nil);
     ibtr:=TpFIBTransaction.Create(nil);
     sqldatos := TpFIBDataSet.Create(nil);
     sqldatos2 := TpFIBDataSet.Create(nil);
     sqldatos3 := TpFIBDataSet.Create(nil);
     sqldatos4 := TpFIBDataSet.Create(nil);
     sqldatos5 := TpFIBDataSet.Create(nil);
     sqldatos6 := TpFIBDataSet.Create(nil);
     FormPreview:=tFormPreview.create(nil);
     Imagen := TMetaFile.Create;
     Imagen2 := TMetaFile.Create;
     sqldatos.Transaction:=ibtr;
     sqldatos2.Transaction:=ibtr;
     sqldatos3.Transaction:=ibtr;
     sqldatos4.Transaction:=ibtr;
     sqldatos5.Transaction:=ibtr;
     sqldatos6.Transaction:=ibtr;
     ibtr.DefaultDatabase:=datamodule.CustomerData.FBase;
     IbTr.BeforeStart:=CustomerData.BaseBeforeStart;
     ImageJPG := TPicture.Create;
     PlantillaJpg:= TPicture.Create;
end;

procedure TClassImpresiones.SetFont(const font: String; const size: integer; const Style: TFontStyles);
begin
  FormPreview.VistaPrevia1.Canvas.Font.Name:=font;
  FormPreview.VistaPrevia1.Canvas.Font.Size:=size;
  FormPreview.VistaPrevia1.Canvas.Font.Style:=Style;
end;

function Imp_Mesas_citacion_pie(ClassImpresiones:TclassImpresiones;fila:real; firma, Firmaimg:String):real;
var
  textosalida:widestring;
  imgfirma: TPicture;

begin
 textosalida:='Es obligación de los señores profesores asistir puntualmente a las/los evaluaciones/exámenes a que sean convocados por la Superiodidad, '+
               'entendiéndose, además, que toda inasistencia no justificada será considerada doble (Art. 53. del Reglamento General). Cualquier objeción al '+
               'presente horario, deberá formularse dentro de los tres días de recibido; en caso contrario, se tendrá por definitivamente aceptado. Si el Señor '+
               'profesor desempeña tareas en otro establecimiento, se servirá presentar al mismo esta comunicación a sus efectos.';
  with FormPreview.VistaPrevia1.Canvas do begin
     Pen.Width:=1;
     Pen.Color:=ClWhite;
     fila:=fila+1.2;
     textout(1,fila,'NOTIFICADO:.......................................',GmCentimeters);
     fila:=fila+0.8;
     textout(1,fila,'FECHA: ___/___/_____',GmCentimeters);
     fila:=fila+1;
     textout(1,fila,'Saluda a Ud. atentamente:',GmCentimeters);
     fila:=fila+0.5;
     imgfirma := TPicture.Create;

     if (firma='R') then begin
         if (Firmaimg='S') then begin
          imgFirma.LoadFromFile(UnidadPrograma+CARPETA_FIRMAS+'\firma_recto.jpg');
          StretchDraw(10,(fila-3),15,fila,imgFirma.Graphic, GmCentimeters);
         end;
         textout(10,fila,FuncionesConfiguracion.Rector,GmCentimeters);
         fila:=fila+0.5;
         textout(11,fila,'RECTOR/A',GmCentimeters);
         fila:=fila+0.8;
     end
     else if (firma='S') then begin
         if (Firmaimg='S') then begin
            imgFirma.LoadFromFile(UnidadPrograma+CARPETA_FIRMAS+'\firma_secre.jpg');
             StretchDraw(10,(fila-3),15,fila,imgFirma.Graphic, GmCentimeters);
         end;
         textout(10,fila,FuncionesConfiguracion.Secretaria,GmCentimeters);
         fila:=fila+0.5;
         textout(11,fila,'SECRETARIA',GmCentimeters);
         fila:=fila+0.8;
     end
     else if (firma='D') then begin
         if (Firmaimg='S') then begin
           imgFirma.LoadFromFile(UnidadPrograma+CARPETA_FIRMAS+'\firma_direc.jpg');
            StretchDraw(10,(fila-3),15,fila,imgFirma.Graphic, GmCentimeters);
         end;
         textout(10,fila,FuncionesConfiguracion.DirEst,GmCentimeters);
         fila:=fila+0.5;
         textout(11,fila,'DIR. DE ESTUDIOS',GmCentimeters);
         fila:=fila+0.8;
     end;
     Font.Size:=9;
     TextBox(1,fila,20,fila+2,textosalida,taLeftJustify,GmCentimeters);
  end
end;

function Imp_Mesas_citacion_enc(ClassImpresiones:TclassImpresiones; copia :string; fila:real):real;
begin
  with FormPreview.VistaPrevia1.Canvas do begin
      Pen.Width:=1;
      Pen.Color:=ClWhite;
      Font.Name := 'Arial';
      Font.Size := 12;
      Font.Style := [FsBold];
      StretchDraw(0,0,21,29.5,ClassImpresiones.PlantillaJpg.Graphic,GmCentimeters);
      textout(1.5,fila,copia,GmCentimeters);
      fila:=fila+0.8;
      textout(3.5,fila,'HORARIO DE EVALUACIONES - EXAMENES PARA PROFESORES',GmCentimeters);
      fila:=fila+0.8;
      textout(2.7,fila,'ESTUDIOS SUPERIORES DE BUENOS AIRES A-781   EMISION:'+FormatDateTime('dd/mm/yyyy',date),GmCentimeters);
      fila:=fila+0.8;
      Font.Size := 10;
      Font.Style := [FsBold];
      textout(1,fila,'Señor Profesor: '+ClassImpresiones.sqldatos.fieldbyname('DOCENTE').AsString,GmCentimeters);
      fila:=fila+0.8;
      Font.Style := [];
      TextBox(1,fila,19,fila+1.5,'Tengo el agrado de dirigime a Ud. comunicándole la fecha de reunión de las mesas de evaluación / examen que debera integrar:',taLeftJustify,GmCentimeters);
      fila:=fila+TextBoxHeight(19,'Tengo el agrado de dirigime a Ud. comunicándole la fecha de reunión de las mesas de evaluación / examen que debera integrar:',GmCentimeters).AsCentimeters;
      fila:=fila+0.2;
      SetPenValues(4,clblack,psSolid);
      MoveTo(1,fila,gmcentimeters);
      LineTo(20,fila,gmcentimeters);
      fila:=fila+0.1;
      Font.Style := [FsBold];
      textout(2,fila,'FECHA',GmCentimeters);
      textout(4,fila,'HORA',GmCentimeters);
      textout(5.5,fila,'ASIGNATURA',GmCentimeters);
      textout(12,fila,'MESA',GmCentimeters);
      textout(14,fila,'AULA',GmCentimeters);
      textout(16,fila,'CARRERA',GmCentimeters);
      fila:=fila+0.5;
      MoveTo(1,fila,gmcentimeters);
      LineTo(20,fila,gmcentimeters);
      fila:=fila+0.2;
      Font.Style := [];
  end;
  Imp_Mesas_citacion_enc:=fila;
end;

procedure Imp_Mesas_citacion(CONST docdde, dochta, fecdde, fechta, Carre, vunidad, firma, Firmaimg :string);
var
    where:string;
    fila:real;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=21;
  if ClassImpresiones.printdialog.Execute then begin
     where:='';
     if (docdde<>'') and (dochta<>'') then
        where:='AND D.CODPROFES BETWEEN '+#39+docdde+#39+' AND '+#39+dochta+#39;
     if (carre<>'') then begin
        where:=where+' AND S.CARRE in ('+#39+AnsiReplaceText(Carre,',',#39','#39)+#39+') ';
     end;
     ClassImpresiones.SQLDATOS.SQLs.SelectSQL.Text:='SELECT DISTINCT D.DOCENTE,D.CODPROFES '+
                                   'FROM MESAS S '+
                                   'LEFT OUTER JOIN DOCENTES D ON D.CODPROFES=S.TITULAR OR D.CODPROFES=S.VOCAL1 OR D.CODPROFES=S.VOCAL2 '+
                                   'WHERE S.FECH_EXA BETWEEN '+#39+fecdde+#39+' AND '+#39+fechta+#39+ where+
                                   'ORDER BY 1';
     ClassImpresiones.ibtr.Active:=True;
     ClassImpresiones.sqldatos.Active:=True;
     if (ClassImpresiones.sqldatos.RecordCount=0) then begin
         MessageDlg('No hay datos para mostrar',mtError,[mbok],0,mbok);
         exit;
     end;
     ClassImpresiones.PlantillaJpg.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\membrete_con_direccion.jpg');
     FormPreview.VistaPrevia1.PaperSize:=A4;
     ClassImpresiones.sqldatos.First;
     where:='';
     if (carre<>'') then
        where:=' AND S.CARRE in ('+#39+AnsiReplaceText(Carre,',',#39','#39)+#39+') ';
     while not (ClassImpresiones.sqldatos.eof) do begin
           ClassImpresiones.sqldatos2.Active:=False;
           ClassImpresiones.SQLDATOS2.SQLs.SelectSQL.Text:='SELECT S.FECH_EXA, SUBSTRING(S.HORA FROM 1 FOR CHAR_LENGTH(S.HORA)-2)||'+#39+':'+#39+'||SUBSTRING(S.HORA FROM CHAR_LENGTH(S.HORA)-1 FOR 2) AS HORA, '+
                                         'IIF(COALESCE(M.SIGLA,'+#39+#39+')='+#39+#39+',SUBSTRING(M.DESCRIPCI FROM 1 FOR 30),M.SIGLA) AS SIGLA, S.MESA, S.AULA, S.CARRE '+
                                         'FROM MESAS S '+
                                         'LEFT OUTER JOIN MATERIAS M ON M.CODMATERI=S.COD_MAT AND M.CODCARRE=S.CARRE '+
                                         'LEFT OUTER JOIN DOCENTES D ON D.CODPROFES=S.TITULAR OR D.CODPROFES=S.VOCAL1 OR D.CODPROFES=S.VOCAL2 '+
                                         'WHERE S.FECH_EXA BETWEEN '+#39+fecdde+#39+' AND '+#39+fechta+#39+ ' AND D.CODPROFES='+#39+ClassImpresiones.sqldatos.fieldbyname('codprofes').AsString+#39+where+
                                         ' ORDER BY 1,2,3';
          ClassImpresiones.sqldatos2.Active:=true;
          ClassImpresiones.sqldatos2.First;
          fila:= 4;
          fila:=Imp_Mesas_citacion_enc(ClassImpresiones,'O R I G I N A L',fila);
          while Not ClassImpresiones.SqlDatos2.eof do begin
              fila:=fila+0.5;
              with FormPreview.VistaPrevia1.Canvas do begin
                   Font.Name := 'Arial';
                   Font.Size := 10;
                   Font.Style := [];
                   textout(1.5,fila,FormatDateTime('dd/mm/yyyy',ClassImpresiones.SqlDatos2.fieldbyname('FECH_EXA').AsDatetime),GmCentimeters);
                   textout(4,fila,ClassImpresiones.SqlDatos2.fieldbyname('HORA').AsString,GmCentimeters);
                   textout(5.5,fila,ClassImpresiones.SqlDatos2.fieldbyname('SIGLA').AsString,GmCentimeters);
                   textout(12,fila,IntToStr(ClassImpresiones.SqlDatos2.fieldbyname('MESA').AsInteger),GmCentimeters);
                   textout(14,fila,IntToStr(ClassImpresiones.SqlDatos2.fieldbyname('AULA').AsInteger),GmCentimeters);
                   textout(16,fila,ClassImpresiones.SqlDatos2.fieldbyname('CARRE').AsString,GmCentimeters);
              end;
              ClassImpresiones.sqldatos2.next;
              if (fila>=ClassImpresiones.largopagina) then begin
                  fila:=Imp_Mesas_citacion_pie(ClassImpresiones,fila,firma,Firmaimg);
                  FormPreview.VistaPrevia1.NewPage;
                  fila:= 4;
                  fila:=Imp_Mesas_citacion_enc(ClassImpresiones,'O R I G I N A L',fila);
              end;
          end;
          fila:=Imp_Mesas_citacion_pie(ClassImpresiones,fila,firma,Firmaimg);
          FormPreview.VistaPrevia1.NewPage;
          fila:= 4;
          fila:=Imp_Mesas_citacion_enc(ClassImpresiones,'D U P L I C A D O',fila);
          ClassImpresiones.Sqldatos2.First;
          while Not ClassImpresiones.sqldatos2.eof do begin
              fila:=fila+0.5;
              with FormPreview.VistaPrevia1.Canvas do begin
                   Font.Name := 'Arial';
                   Font.Size := 10;
                   Font.Style := [];
                   textout(1.5,fila,FormatDateTime('dd/mm/yyyy',ClassImpresiones.SqlDatos2.fieldbyname('FECH_EXA').AsDatetime),GmCentimeters);
                   textout(4,fila,ClassImpresiones.SqlDatos2.fieldbyname('HORA').AsString,GmCentimeters);
                   textout(5.5,fila,ClassImpresiones.SqlDatos2.fieldbyname('SIGLA').AsString,GmCentimeters);
                   textout(12,fila,IntToStr(ClassImpresiones.SqlDatos2.fieldbyname('MESA').AsInteger),GmCentimeters);
                   textout(14,fila,IntToStr(ClassImpresiones.SqlDatos2.fieldbyname('AULA').AsInteger),GmCentimeters);
                   textout(16,fila,ClassImpresiones.SqlDatos2.fieldbyname('CARRE').AsString,GmCentimeters);
              end;
              ClassImpresiones.SqlDatos2.next;
              if (fila>=ClassImpresiones.largopagina) then begin
                  fila:=Imp_Mesas_citacion_pie(ClassImpresiones,fila,firma, firmaimg);
                  FormPreview.VistaPrevia1.NewPage;
                  fila:= 4;
                  fila:=Imp_Mesas_citacion_enc(ClassImpresiones,'D U P L I C A D O',fila);
              end;
          end;
          fila:=Imp_Mesas_citacion_pie(classImpresiones,fila,firma, firmaimg);
          ClassImpresiones.sqldatos.Next;
          if not ClassImpresiones.sqldatos.Eof then begin
             FormPreview.VistaPrevia1.NewPage;
          end;
     end;
     FormPreview.VistaPrevia1.CurrentPage := 1;
     FormPreview.showmodal;
     ClassImpresiones.Finaliza;
  end;
end;

procedure Imp_Mesas_ParteDiario(CONST fecdde, fechta, fcarre, vunidad :string);
var
    fecha_aux:tDatetime;
    fila:real;
    carre:String;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=27;
  ClassImpresiones.margen:=0.6;
  if ClassImpresiones.printdialog.Execute then begin
     ClassImpresiones.SQLDATOS.SQLs.SelectSQL.Text:= 'SELECT TRIM(COALESCE(SUBSTRING(D1.DOCENTE FROM 1 FOR 15),'+#39+#39+'))||'+#39+'-'+#39+'||TRIM(COALESCE(SUBSTRING(D2.DOCENTE FROM 1 FOR 15),'+#39+#39+'))||'+#39+'-'+#39+'||TRIM(COALESCE(SUBSTRING(D3.DOCENTE  FROM 1 FOR 15),'+#39+#39+')) AS DOCENTE, S.FECH_EXA, '+
                                                     'SUBSTRING(S.HORA FROM 1 FOR CHAR_LENGTH(S.HORA)-2)||'+#39+':'+#39+'||SUBSTRING(S.HORA FROM CHAR_LENGTH(S.HORA)-1 FOR 2) AS HORA, '+
                                                     'IIF(COALESCE(M.SIGLA,'+#39+#39+')='+#39+#39+',SUBSTRING(M.DESCRIPCI FROM 1 FOR 30),M.SIGLA) AS SIGLA, S.MESA, S.AULA, S.CARRE, (SELECT COUNT(*) FROM PERMEXA P WHERE P.CARRE=S.CARRE AND S.MESA=P.MESA AND P.FECH_EXA=S.FECH_EXA) AS CANALU, '+
                                                     'COALESCE(S.COMI1,0)||'+#39+'/'+#39+'||COALESCE(S.COMI3,0)||'+#39+'/'+#39+'||COALESCE(S.COMI3,0) AS COMISION '+
                                                     'FROM MESAS S '+
                                                     'LEFT OUTER JOIN MATERIAS M ON M.CODMATERI=S.COD_MAT AND M.CODCARRE=S.CARRE '+
                                                     'LEFT OUTER JOIN DOCENTES D1 ON D1.CODPROFES=S.TITULAR '+
                                                     'LEFT OUTER JOIN DOCENTES D2 ON D2.CODPROFES=S.VOCAL1 '+
                                                     'LEFT OUTER JOIN DOCENTES D3 ON D3.CODPROFES=S.VOCAL2 '+
                                                     'WHERE S.FECH_EXA BETWEEN '+#39+fecdde+#39+' AND '+#39+fechta+#39+' AND (S.CARRE='+#39+fcarre+#39+' OR '+#39+#39+'='+#39+fcarre+#39+') '+
                                                     'ORDER BY S.FECH_EXA,S.CARRE,S.HORA';
     ClassImpresiones.ibtr.Active:=True;
     ClassImpresiones.sqldatos.Active:=True;
     if (ClassImpresiones.sqldatos.RecordCount=0) then begin
         MessageDlg('No hay datos para mostrar',mtError,[mbok],0,mbok);
         exit;
     end;
     FormPreview.VistaPrevia1.PaperSize:=A4;
     ClassImpresiones.sqldatos.First;
     fecha_aux:=strtodate('01/01/2000');
     Carre:='';
     while not (ClassImpresiones.sqldatos.eof) do begin
              if (ClassImpresiones.SqlDatos.fieldbyname('FECH_EXA').AsDatetime<>fecha_aux) then begin
                   fila:=2;
                   if fecha_aux<>strtodate('01/01/2000') then
                       FormPreview.VistaPrevia1.NewPage;
                   fecha_aux:=ClassImpresiones.SqlDatos.fieldbyname('FECH_EXA').AsDatetime;
                   carre:=ClassImpresiones.SqlDatos.fieldbyname('CARRE').AsString;
                   with FormPreview.VistaPrevia1.Canvas do begin
                          Font.Name := 'Arial';
                          Font.Size := 12;
                          Font.Style := [FsBold];
                          textout(5,fila,'MESAS DE EXAMENES FINALES - PARTE DIARIO',GmCentimeters);
                          fila:=fila+0.8;
                          textout(8,fila,'CURSO LECTIVO: '+FormatDateTime('yyyy',DATE),GmCentimeters);
                          fila:=fila+0.8;
                          textout(ClassImpresiones.margen+2,fila,'Fecha de examen: '+FormatDateTime('dd/mm/yyyy',fecha_aux),GmCentimeters);
                          fila:=fila+0.8;
                          Font.Size := 9;
                          textout(ClassImpresiones.margen+1,fila,'MESA',GmCentimeters);
                          textout(ClassImpresiones.margen+2.2,fila,'HORA',GmCentimeters);
                          textout(ClassImpresiones.margen+3.4,fila,'MATERIA',GmCentimeters);
                          textout(ClassImpresiones.margen+7.2,fila,'PROFESORES',GmCentimeters);
                          textout(ClassImpresiones.margen+14.7,fila,'COMISION',GmCentimeters);
                          textout(ClassImpresiones.margen+16.7,fila,'CANT.ALUM',GmCentimeters);
                          textout(ClassImpresiones.margen+18.9,fila,'AULA',GmCentimeters);
                          fila:=fila+0.5;
                          SetPenValues(4,clblack,psSolid);
                          MoveTo(ClassImpresiones.margen+1,fila,gmcentimeters);
                          LineTo(ClassImpresiones.margen+20,fila,gmcentimeters);
                          fila:=fila+0.1;
                          Font.Name := 'Arial';
                          Font.Size := 10;
                          Font.Style := [FsBold];
                          textout(ClassImpresiones.margen+2.5,fila,ClassImpresiones.SqlDatos.fieldbyname('CARRE').AsString,GmCentimeters);
                          fila:=fila+0.5;
                          SetPenValues(4,clblack,psSolid);
                          MoveTo(ClassImpresiones.margen+1,fila,gmcentimeters);
                          LineTo(ClassImpresiones.margen+20,fila,gmcentimeters);
                          //fila:=fila+0.3;
                    end;
              end;
              if (carre<>ClassImpresiones.SqlDatos.fieldbyname('CARRE').AsString) then begin
                 fila:=fila+0.3;
                 carre:=ClassImpresiones.SqlDatos.fieldbyname('CARRE').AsString;
                 with FormPreview.VistaPrevia1.Canvas do begin
                     Font.Name := 'Arial';
                     Font.Size := 10;
                     Font.Style := [FsBold];
                     fila:=fila+0.5;
                     SetPenValues(4,clblack,psSolid);
                     MoveTo(ClassImpresiones.margen+1,fila,gmcentimeters);
                     LineTo(ClassImpresiones.margen+20,fila,gmcentimeters);
                     fila:=fila+0.1;
                     textout(ClassImpresiones.margen+2.5,fila,ClassImpresiones.SqlDatos.fieldbyname('CARRE').AsString,GmCentimeters);
                     fila:=fila+0.5;
                     SetPenValues(4,clblack,psSolid);
                     MoveTo(ClassImpresiones.margen+1,fila,gmcentimeters);
                     LineTo(ClassImpresiones.margen+20,fila,gmcentimeters);
//                     fila:=fila+0.3;
                 end;
                 //fila:=fila+0.3;
              end;
              fila:=fila+0.5;
              with FormPreview.VistaPrevia1.Canvas do begin
                   Font.Name := 'Arial';
                   Font.Size := 8;
                   Font.Style := [];
                   textout(ClassImpresiones.margen+1,fila,inttostr(ClassImpresiones.SqlDatos.fieldbyname('MESA').AsInteger),GmCentimeters);
                   textout(ClassImpresiones.margen+2.2,fila,ClassImpresiones.SqlDatos.fieldbyname('HORA').AsString,GmCentimeters);
                   textout(ClassImpresiones.margen+3.2,fila,ClassImpresiones.SqlDatos.fieldbyname('SIGLA').AsString,GmCentimeters);
                   textout(ClassImpresiones.margen+6.7,fila,ClassImpresiones.SqlDatos.fieldbyname('DOCENTE').AsString,GmCentimeters);
                   textout(ClassImpresiones.margen+15.2,fila,ClassImpresiones.SqlDatos.fieldbyname('COMISION').AsString,GmCentimeters);
                   textout(ClassImpresiones.margen+17.2,fila,IntToStr(ClassImpresiones.SqlDatos.fieldbyname('CANALU').AsInteger),GmCentimeters);
                   textout(ClassImpresiones.margen+19.2,fila,IntTostr(ClassImpresiones.SqlDatos.fieldbyname('AULA').AsInteger),GmCentimeters);
              end;
              ClassImpresiones.SqlDatos.next;
              if (fila>=ClassImpresiones.largopagina) then begin
                   fila:=2;
                   FormPreview.VistaPrevia1.NewPage;
                   with FormPreview.VistaPrevia1.Canvas do begin
                          Font.Name := 'Arial';
                          Font.Size := 12;
                          Font.Style := [FsBold];
                          textout(5,fila,'MESAS DE EXAMENES FINALES - PARTE DIARIO',GmCentimeters);
                          fila:=fila+0.8;
                          textout(8,fila,'CURSO LECTIVO: '+FormatDateTime('yyyy',DATE),GmCentimeters);
                          fila:=fila+0.8;
                          textout(ClassImpresiones.margen+2,fila,'Fecha de examen: '+FormatDateTime('dd/mm/yyyy',fecha_aux),GmCentimeters);
                          fila:=fila+0.8;
                          Font.Size := 9;
                          textout(ClassImpresiones.margen+1,fila,'MESA',GmCentimeters);
                          textout(ClassImpresiones.margen+2.2,fila,'HORA',GmCentimeters);
                          textout(ClassImpresiones.margen+3.4,fila,'MATERIA',GmCentimeters);
                          textout(ClassImpresiones.margen+7.2,fila,'PROFESORES',GmCentimeters);
                          textout(ClassImpresiones.margen+14.7,fila,'COMISION',GmCentimeters);
                          textout(ClassImpresiones.margen+16.7,fila,'CANT.ALUM',GmCentimeters);
                          textout(ClassImpresiones.margen+18.9,fila,'AULA',GmCentimeters);
                          fila:=fila+0.5;
                          SetPenValues(4,clblack,psSolid);
                          MoveTo(ClassImpresiones.margen+1,fila,gmcentimeters);
                          LineTo(classimpresiones.margen+20,fila,gmcentimeters);
                          fila:=fila+0.3;
                    end;
              end;
     end;
     formpreview.ShowModal;
  end;
end;

Procedure Imp_Recincorp_todos(Const MtFaltas:TKbmMemtable; Vunidad, Carrera, Vinstituto,codalu:String);
Var
   Fila:Real;
   textosalida:WideString;

   procedure imprimir;
   begin

      with FormPreview.VistaPrevia1.Canvas do begin
           Font.Name := 'Arial';
           Font.Size := 12;
           Font.Style := [FsBold];
           textout((20-TextWidth('SOLICITUD DE REINCORPORACION POR INASISTENCIAS').AsCentimeters)/2,fila,'SOLICITUD DE REINCORPORACION POR INASISTENCIAS',GmCentimeters);
           fila:=fila+0.8;
           textout((20-TextWidth(carrera).AsCentimeters)/2,fila,Carrera,GmCentimeters);
           fila:=fila+0.8;
           Font.Size := 10;
           textout(15,fila,'Bs. As. '+FormatDateTime('dd "de" mmmm "de" yyyy',date),GmCentimeters);
           fila:=fila+0.8;
           textout(2,fila,'Sr/a Rector/a',GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,VInstituto,GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,FuncionesConfiguracion.Rector,GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,'S......../..........D',GmCentimeters);
           fila:=fila+1;
           Font.Style := [];
           textosalida:='Por la presente solicito a UD., tenga bien, reincorporar por inasistenacias a mi '+
                        'hijo/a: '+MtFaltas.FieldByName('NOMBRE').Value+ ' de '+copy(IntToStr(MtFaltas.FieldByName('CUTUCO').Value),1,1)+'° '+
                        'año, división '+ FuncionesText.Division(StrToInt(copy(IntToStr(MtFaltas.FieldByName('CUTUCO').Value),3,1)))+', turno '+
                        FuncionesText.Turnos(StrToInt(copy(IntToStr(MtFaltas.FieldByName('CUTUCO').Value),2,1)))+' - Ya que el día '+
                        FormatDateTime('dd/mm/yyyy',MtFaltas.FieldByName('FECHA').Value)+ ' quedó libre '+MtFaltas.FieldByName('PRISEG').Value+'.';
           TextBox(2,fila,19,fila+1.5,TEXTOSALIDA,taLeftJustify,GmCentimeters);
           fila:=fila+TextBoxHeight(19,textosalida,GmCentimeters).AsCentimeters+1;
           textout(2,fila,'Atentamente',GmCentimeters);
           fila:=fila+1;
           Font.Style := [FsBold];
           textout(2,fila,'Firma del padre/madre: ____________________________',GmCentimeters);
           fila:=fila+0.9;
           textout(2,fila,'Aclaración: ____________________________',GmCentimeters);
           fila:=fila+1;
           textout(2,fila,funcionesText.LPad('',160,'.'),GmCentimeters);
           fila:=fila+0.7;
           Font.Style:=[FsBold];
           textout((20-TextWidth('SECRETARIA DOCENTE').AsCentimeters)/2,fila,'SECRETARIA DOCENTE',GmCentimeters);
           fila:=fila+1;
           Font.Style:=[];
           textout(2,fila,'Fecha: '+FormatDateTime('dd/mm/yyyy',date),GmCentimeters);
           fila:=fila+1;
           textosalida:='A la decha el/la alumno/a: '+MtFaltas.FieldByName('NOMBRE').Value+ ' de '+copy(IntToStr(MtFaltas.FieldByName('CUTUCO').Value),1,1)+'° '+
                        'año, división '+ FuncionesText.Division(StrToInt(copy(IntToStr(MtFaltas.FieldByName('CUTUCO').Value),2,1)))+', turno '+
                        FuncionesText.Turnos(StrToInt(copy(IntToStr(MtFaltas.FieldByName('CUTUCO').Value),3,1)))+' quedó LIBRE POR INASISTENCIAS '+ MtFaltas.FieldByName('PRISEG').Value+
                        '. Registrando los siguientes antecedentes disciplinarios y acádemicos.';
           TextBox(2,fila,19,fila+1.5,TEXTOSALIDA,taLeftJustify,GmCentimeters);
           fila:=fila+TextBoxHeight(19,textosalida,GmCentimeters).AsCentimeters+1;
           textout(2,fila,'1- Inasistencias justificadas: ' + FloatTostr(MtFaltas.fieldByName('INAJUS').Value),GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,'2- Inasistencias in-justificadas: ' + FloatTostr(MtFaltas.fieldByName('INAINJUS').Value),GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,'3- Inasistencias a Ed. Física (solo a Ed. Física):' + FloatTostr(MtFaltas.fieldByName('INAEDFIS').Value)  ,GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,'4- Llegadas tarde: ' + FloatTostr(MtFaltas.fieldByName('INATAR').Value) ,GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,'5- Observaciones o Actas disciplinarias: '+ intTostr(MtFaltas.fieldByName('ACTDISC').Value)  ,GmCentimeters);
           fila:=fila+0.5;
           textout(2,fila,'8- Inasistencias a la fecha: ' +FloatTostr(MtFaltas.fieldByName('INATOT').Value) ,GmCentimeters);
           fila:=fila+1;
           Font.Style:=[FsBold];
           textout(2,fila,funcionesText.LPad('',160,'.'),GmCentimeters);
           fila:=fila+0.7;
           textout((20-TextWidth('RESOLUCION GENERAL').AsCentimeters)/2,fila,'RESOLUCION GENERAL',GmCentimeters);
           fila:=fila+0.8;
           Font.Style:=[];
           textout(2,fila,'Fecha: '+FormatDateTime('dd/mm/yyyy',date),GmCentimeters);
           fila:=fila+0.8;
           textout(2,fila,'Observaciones:',GmCentimeters);
           fila:=fila+0.8;
           textout(2,fila,FuncionesText.LPad('',160,'.'),GmCentimeters);
           fila:=fila+0.8;
           textout(2,fila,FuncionesText.LPad('',160,'.'),GmCentimeters);
           fila:=fila+0.8;
           textout(2,fila,FuncionesText.LPad('',160,'.'),GmCentimeters);
      end;
   end;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=27;
  if ClassImpresiones.printdialog.Execute then begin
     if (MtFaltas.RecordCount=0) then begin
         MessageDlg('No hay datos para mostrar',mtError,[mbok],0,mbok);
         exit;
     end;
     ClassImpresiones.PlantillaJpg.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\membrete_con_direccion.jpg');
     FormPreview.VistaPrevia1.Canvas.StretchDraw(0,0,21,29.5,ClassImpresiones.Imagen,GmCentimeters);
     FormPreview.VistaPrevia1.PaperSize:=A4;
     FormPreview.VistaPrevia1.Canvas.Pen.Width:=1;
     FormPreview.VistaPrevia1.Canvas.Pen.Color:=ClWhite;
     Fila:=4;
     if (codalu='') then begin
         MtFaltas.First;
         while (not MtFaltas.eof) do begin
              imprimir;
              MtFaltas.Next;
              if not (mtfaltas.eof) then begin
                  FormPreview.VistaPrevia1.NewPage;
                  Fila:=4;
                  FormPreview.VistaPrevia1.Canvas.StretchDraw(0,0,21,29.5,ClassImpresiones.PlantillaJpg.Graphic,GmCentimeters);
                  //FormPreview.VistaPrevia1.BeginUpdate;
              end;
         end;
     end
     else
      imprimir;
     formpreview.ShowModal;
  end;
  ClassImpresiones.Finaliza;
end;

Procedure Imp_Boletin_calif_enc;
begin
 //aca iria la impresion del encabezado
end;

Procedure Imp_Boletin_calif(carre, codalu, Vunidad, baja :String; anio, cutuco, Sanciones:Integer);
Var
   Fila,TextoHeight :Real;
   textosalida:WideString;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=27;
  if ClassImpresiones.printdialog.Execute then begin
     ClassImpresiones.ImageJPG.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\boletin.jpg');

     ClassImpresiones.Imagen.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\boletin.wmf');
     ClassImpresiones.Imagen2.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\Planilla_Sanciones.wmf');

     FormPreview.VistaPrevia1.PaperSize:=A4;
     FormPreview.VistaPrevia1.Canvas.Pen.Width:=1;
     FormPreview.VistaPrevia1.Canvas.Pen.Color:=ClWhite;
     ClassImpresiones.SqlDatos.SQLs.SelectSQL.Text:='SELECT FCODALU, FANIO, FTURNO, FDIVI, FCICLO, FDOCUMENTO, FNOMBRE, FCUTUCO FROM YYY_IMP_BOLETIN_ALUMNOS('+#39+carre+#39+','+#39+codalu+#39+','+IntToStr(anio)+','+#39+baja+#39+','+IntTostr(cutuco)+')'; //aca va el query que selecciona a los alumnos
     ClassImpresiones.IbTr.Active:=True;
     ClassImpresiones.SqlDatos.Active:=True;
     while not ClassImpresiones.SqlDatos.Eof do begin
            Fila:=2.3;
            with FormPreview.VistaPrevia1.Canvas do begin
               StretchDraw(0,0,20.5,29.5,ClassImpresiones.Imagen,GmCentimeters);
               Font.Name:='Arial';
               Font.Size:=10;
               Font.Style:=[];
               With ClassImpresiones.SqlDatos Do begin
                  TextOut(9.5,Fila,FieldByName('FNOMBRE').Value,GmCentimeters);
                  fila:=fila+0.4;
                  TextOut(8,Fila,FieldByName('FDOCUMENTO').Value,GmCentimeters);
                  fila:=fila+0.5;
                  TextOut(5.8,Fila,IntToStr(FieldByName('FANIO').Value),GmCentimeters);
                  TextOut(8.8,Fila,FieldByName('FTURNO').Value,GmCentimeters);
                  TextOut(12.2,Fila,FieldByName('FDIVI').Value,GmCentimeters);
                  TextOut(19.2,Fila,IntToStr(FieldByName('FCICLO').Value),GmCentimeters);
               end;
            end;
            with ClassImpresiones do begin
                SqlDatos2.Active:=False;
                SqlDatos3.Active:=False;
                SqlDatos4.Active:=False;
                SqlDatos5.Active:=False;
                SqlDatos2.SQLs.SelectSQL.Text:='SELECT FMATERIA, FTRIM1, FTRIM2, FTRIM3, FPROM, FRECUP, FNOTDEF, FCONDI, FESPACI, FLIN1, FLIN2, FLIN3, FLINPRO FROM YYY_IMP_BOLETIN_MATE('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FANIO').Value)+');';
                SqlDatos3.SQLs.SelectSQL.Text:='SELECT FMATERIA, FDIC, FMAR, FLINEA, FESPACI FROM YYY_IMP_BOLETIN_PENDI('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FANIO').Value)+');';
                SqlDatos4.SQLs.SelectSQL.Text:='SELECT FTOTAL, FESPACI FROM YYY_IMP_BOLETIN_FALTAS('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FCICLO').Value)+');';
                SqlDatos5.SQLs.SelectSQL.Text:='SELECT FTOTAL, FTEXTO FROM YYY_IMP_BOLETIN_SANCI('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FCICLO').Value)+');';
                SqlDatos2.Active:=True;
                SqlDatos3.Active:=True;
                SqlDatos4.Active:=True;
                SqlDatos5.Active:=True;


                SqlDatos2.First;
                fila:=6.3;
                with FormPreview.VistaPrevia1.Canvas do begin
                      Font.Name:='Arial';
                      Font.Size:=8;
                      Font.Style:=[];
                      while not SqlDatos2.Eof do begin
                          TextOut(0.9,Fila,ClassImpresiones.SqlDatos2.FieldByName('FMATERIA').Value,GmCentimeters);
                          if (not ClassImpresiones.SqlDatos2.FieldByName('FTRIM1').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FTRIM1').value<>0) then
                                TextOut(7.2,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FTRIM1').Value),GmCentimeters)
                          else
                                TextOut(7.2,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLIN1').Value,GmCentimeters);
                          if (not ClassImpresiones.SqlDatos2.FieldByName('FTRIM2').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FTRIM2').value<>0) then
                                TextOut(8.9,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FTRIM2').Value),GmCentimeters)
                          else
                                TextOut(8.9,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLIN2').Value,GmCentimeters);
                          if (not ClassImpresiones.SqlDatos2.FieldByName('FTRIM3').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FTRIM3').value<>0) then
                                TextOut(10.7,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FTRIM3').Value),GmCentimeters)
                          else
                                TextOut(10.7,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLIN3').Value,GmCentimeters);
                          if (not ClassImpresiones.SqlDatos2.FieldByName('FPROM').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FPROM').value<>0) then
                                 TextOut(12.3,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FPROM').Value),GmCentimeters)
                          else
                                 TextOut(12.3,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLINPRO').Value,GmCentimeters);
//                          TextOut(12.8,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FRECUP').Value),GmCentimeters);
                          TextOut(15,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FNOTDEF').Value),GmCentimeters);
                          TextOut(17.3,Fila,ClassImpresiones.SqlDatos2.FieldByName('FCONDI').Value,GmCentimeters);
                          fila:=fila+ClassImpresiones.SqlDatos2.FieldByName('FESPACI').Value;
                          SqlDatos2.Next;
                      end;
                end;
                SqlDatos2.Active:=False;

                SqlDatos3.First;
                fila:=14.1;
                with FormPreview.VistaPrevia1.Canvas do begin
                      Font.Name:='Arial';
                      Font.Size:=8;
                      Font.Style:=[];
                      while not SqlDatos3.Eof do begin
                          TextOut(0.9,Fila,ClassImpresiones.SqlDatos3.FieldByName('FMATERIA').Value,GmCentimeters);
                          if varisnull(ClassImpresiones.SqlDatos3.FieldByName('FLINEA').Value) then begin
                              TextOut(8,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos3.FieldByName('FDIC').Value),GmCentimeters);
                              TextOut(14.2,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos3.FieldByName('FMAR').Value),GmCentimeters);
                          end else
                              TextOut(7.5,Fila,ClassImpresiones.SqlDatos3.FieldByName('FLINEA').Value,GmCentimeters);
                          fila:=fila+ClassImpresiones.SqlDatos3.FieldByName('FESPACI').Value;
                          SqlDatos3.Next;
                      end;
                end;
                SqlDatos3.Active:=False;

                SqlDatos4.First;
                fila:=25.1;
                with FormPreview.VistaPrevia1.Canvas do begin
                    while not SqlDatos4.Eof do begin
                        TextOut(4.5,Fila,FormatFloat('0.00',ClassImpresiones.SqlDatos4.FieldByName('FTOTAL').Value),GmCentimeters);
                        fila:=fila+ClassImpresiones.SqlDatos4.FieldByName('FESPACI').Value;
                        SqlDatos4.next;
                    end;
                end;
                SqlDatos4.Active:=False;

                SqlDatos5.First;
                with FormPreview.VistaPrevia1.Canvas do begin
                    TextOut(19,24.2,FormatFloat('0',ClassImpresiones.SqlDatos5.FieldByName('FTOTAL').Value),GmCentimeters);
                    Font.Name:='Arial';
                    Font.Size:=7;
                    Font.Style:=[];
                    Textbox(6.1,24.7,20,27,ClassImpresiones.SqlDatos5.FieldByName('FTEXTO').Value,taLeftJustify,GmCentimeters);
                end;
                SqlDatos5.Active:=False;
            end;
            if sanciones=0 then  begin//va con saciones

                ClassImpresiones.SqlDatos6.Active:=False;
                ClassImpresiones.SqlDatos6.SQLs.SelectSQL.Text:='SELECT FECHA, CARPOR, FTEXTO FROM YYY_IMP_BOLETIN_SANCI_DETA('+#39+ClassImpresiones.SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(ClassImpresiones.SqlDatos.FieldByName('FCICLO').Value)+');';
                ClassImpresiones.SqlDatos6.Active:=True;
                if ClassImpresiones.SqlDatos6.RecordCount>0 then begin
                    FormPreview.VistaPrevia1.NewPage;
                    Fila:=2.3;
                    with FormPreview.VistaPrevia1.Canvas do begin
                       StretchDraw(0,0,20.5,29.5,ClassImpresiones.Imagen2,GmCentimeters);
                       Font.Name:='Arial';
                       Font.Size:=10;
                       Font.Style:=[];
                       With ClassImpresiones.SqlDatos Do begin
                          TextOut(9.5,Fila,FieldByName('FNOMBRE').Value,GmCentimeters);
                          fila:=fila+0.4;
                          TextOut(8,Fila,FieldByName('FDOCUMENTO').Value,GmCentimeters);
                          fila:=fila+0.5;
                          TextOut(5.8,Fila,IntToStr(FieldByName('FANIO').Value),GmCentimeters);
                          TextOut(8.8,Fila,FieldByName('FTURNO').Value,GmCentimeters);
                          TextOut(12.2,Fila,FieldByName('FDIVI').Value,GmCentimeters);
                          TextOut(19.2,Fila,IntToStr(FieldByName('FCICLO').Value),GmCentimeters);
                       end;
                    end;
                    fila:=5;
                    while not ClassImpresiones.SqlDatos6.Eof do begin
                         with FormPreview.VistaPrevia1.Canvas do begin
                           Font.Name:='Arial';
                           Font.Size:=11;
                           Font.Style:=[fsBold];
                           textosalida:='Fecha: '+FormatDateTime('dd/mm/yyyy',ClassImpresiones.SqlDatos6.FieldByName('FECHA').Value)+' Otorgada por: '+ClassImpresiones.SqlDatos6.FieldByName('CARPOR').AsString;
                           TextOut(0.9,Fila,textosalida,GmCentimeters);
                           fila:=fila+0.5;
                           Font.Size:=9;
                           Font.Style:=[];
                           textosalida:=ClassImpresiones.SqlDatos6.FieldByName('FTEXTO').Value;
                           TextoHeight:=TextBoxHeight(18,textosalida,GmCentimeters).AsCentimeters;
                           Textbox(1.5,fila,20,fila+TextoHeight,textosalida,taLeftJustify,GmCentimeters);
                           fila:=fila+TextoHeight;
                         end;
                         ClassImpresiones.SqlDatos6.next;
                    end;
                end;
                ClassImpresiones.SqlDatos6.Active:=False;
            end;
       ClassImpresiones.SqlDatos.Next;
       if not ClassImpresiones.SqlDatos.eof then
          FormPreview.VistaPrevia1.NewPage;
     end;
     formpreview.ShowModal;
  end;
  ClassImpresiones.Finaliza;
end;

Procedure Imp_Boletin_calif_formatonuevo(carre, codalu, Vunidad, baja :String; anio, cutuco, Sanciones:Integer);
Var
   Fila,TextoHeight :Real;
   textosalida:WideString;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=27;
  if ClassImpresiones.printdialog.Execute then begin
     ClassImpresiones.ImageJPG.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\boletinnuevo.jpg');

     //ClassImpresiones.Imagen.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\boletinnuevo.emf');
     ClassImpresiones.Imagen2.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\Planilla_Sanciones.wmf');

     FormPreview.VistaPrevia1.PaperSize:=A4;
     FormPreview.VistaPrevia1.Canvas.Pen.Width:=1;
     FormPreview.VistaPrevia1.Canvas.Pen.Color:=ClWhite;
     ClassImpresiones.SqlDatos.SQLs.SelectSQL.Text:='SELECT FCODALU, FANIO, FTURNO, FDIVI, FCICLO, FDOCUMENTO, FNOMBRE, FCUTUCO FROM YYY_IMP_BOLETIN_ALUMNOS('+#39+carre+#39+','+#39+codalu+#39+','+IntToStr(anio)+','+#39+baja+#39+','+IntTostr(cutuco)+')'; //aca va el query que selecciona a los alumnos
     ClassImpresiones.IbTr.Active:=True;
     ClassImpresiones.SqlDatos.Active:=True;
     while not ClassImpresiones.SqlDatos.Eof do begin
            Fila:=2.3;
            with FormPreview.VistaPrevia1.Canvas do begin
               StretchDraw(0.4,0.6,20.5,29.5,ClassImpresiones.ImageJPG.Graphic ,GmCentimeters);
               Font.Name:='Arial';
               Font.Size:=10;
               Font.Style:=[];
               With ClassImpresiones.SqlDatos Do begin
                  TextOut(9.5,Fila,FieldByName('FNOMBRE').Value,GmCentimeters);
                  fila:=fila+0.4;
                  TextOut(8,Fila,FieldByName('FDOCUMENTO').Value,GmCentimeters);
                  fila:=fila+0.5;
                  TextOut(5.8,Fila,IntToStr(FieldByName('FANIO').Value),GmCentimeters);
                  TextOut(8.8,Fila,FieldByName('FTURNO').Value,GmCentimeters);
                  TextOut(12.2,Fila,FieldByName('FDIVI').Value,GmCentimeters);
                  TextOut(19.2,Fila,IntToStr(FieldByName('FCICLO').Value),GmCentimeters);
               end;
            end;
            with ClassImpresiones do begin
                SqlDatos2.Active:=False;
                SqlDatos3.Active:=False;
                SqlDatos4.Active:=False;
                SqlDatos5.Active:=False;
                SqlDatos2.SQLs.SelectSQL.Text:='SELECT FMATERIA, FTRIM1, FTRIM2, FTRIM3, NULL AS FPROM, FRECUP, FNOTDEF, FCONDI, FESPACI, FLIN1, FLIN2, FLIN3, FLINPRO FROM YYY_IMP_BOLETIN_MATE('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FANIO').Value)+');';
                SqlDatos3.SQLs.SelectSQL.Text:='SELECT FMATERIA, FDIC, FMAR, FLINEA, FESPACI FROM YYY_IMP_BOLETIN_PENDI_NEW('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FANIO').Value)+');';
                SqlDatos4.SQLs.SelectSQL.Text:='SELECT FTOTAL, FESPACI FROM YYY_IMP_BOLETIN_FALTAS('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FCICLO').Value)+');';
                SqlDatos5.SQLs.SelectSQL.Text:='SELECT FTOTAL, FTEXTO FROM YYY_IMP_BOLETIN_SANCI('+#39+SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(SqlDatos.FieldByName('FCICLO').Value)+');';
                SqlDatos2.Active:=True;
                SqlDatos3.Active:=True;
                SqlDatos4.Active:=True;
                SqlDatos5.Active:=True;


                SqlDatos2.First;
                fila:=6.3;
                with FormPreview.VistaPrevia1.Canvas do begin
                      Font.Name:='Arial';
                      Font.Size:=8;
                      Font.Style:=[];
                      while not SqlDatos2.Eof do begin
                          TextOut(0.9,Fila,ClassImpresiones.SqlDatos2.FieldByName('FMATERIA').Value,GmCentimeters);
                          if (not ClassImpresiones.SqlDatos2.FieldByName('FTRIM1').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FTRIM1').value<>0) then
                                TextOut(7.2,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FTRIM1').Value),GmCentimeters)
                          else
                                TextOut(7.2,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLIN1').Value,GmCentimeters);
                          if (not ClassImpresiones.SqlDatos2.FieldByName('FTRIM2').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FTRIM2').value<>0) then
                                TextOut(8.9,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FTRIM2').Value),GmCentimeters)
                          else
                                TextOut(8.9,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLIN2').Value,GmCentimeters);
                          {if (not ClassImpresiones.SqlDatos2.FieldByName('FTRIM3').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FTRIM3').value<>0) then
                                TextOut(10.7,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FTRIM3').Value),GmCentimeters)
                          else
                                TextOut(10.7,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLIN3').Value,GmCentimeters);

                          if (not ClassImpresiones.SqlDatos2.FieldByName('FPROM').IsNull) and (ClassImpresiones.SqlDatos2.FieldByName('FPROM').value<>0) then
                                 TextOut(12.3,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FPROM').Value),GmCentimeters)
                          else
                                 TextOut(12.3,Fila,ClassImpresiones.SqlDatos2.FieldByName('FLINPRO').Value,GmCentimeters);
                          }
//                          TextOut(12.8,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FRECUP').Value),GmCentimeters);
                          TextOut(15,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FNOTDEF').Value),GmCentimeters);
                          TextOut(17.3,Fila,ClassImpresiones.SqlDatos2.FieldByName('FCONDI').Value,GmCentimeters);
                          fila:=fila+ClassImpresiones.SqlDatos2.FieldByName('FESPACI').Value;
                          SqlDatos2.Next;
                      end;
                end;
                SqlDatos2.Active:=False;

                SqlDatos3.First;
                fila:=14.1;
                with FormPreview.VistaPrevia1.Canvas do begin
                      Font.Name:='Arial';
                      Font.Size:=8;
                      Font.Style:=[];
                      while not SqlDatos3.Eof do begin
                          TextOut(0.9,Fila,ClassImpresiones.SqlDatos3.FieldByName('FMATERIA').Value,GmCentimeters);
                          if varisnull(ClassImpresiones.SqlDatos3.FieldByName('FLINEA').Value) then begin
                              TextOut(8,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos3.FieldByName('FDIC').Value),GmCentimeters);
                              TextOut(14.2,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos3.FieldByName('FMAR').Value),GmCentimeters);
                          end else
                              TextOut(7.5,Fila,ClassImpresiones.SqlDatos3.FieldByName('FLINEA').Value,GmCentimeters);
                          fila:=fila+ClassImpresiones.SqlDatos3.FieldByName('FESPACI').Value;
                          SqlDatos3.Next;
                      end;
                end;
                SqlDatos3.Active:=False;

                SqlDatos4.First;
                fila:=25.1;
                with FormPreview.VistaPrevia1.Canvas do begin
                    while not SqlDatos4.Eof do begin
                        TextOut(4.5,Fila,FormatFloat('0.00',ClassImpresiones.SqlDatos4.FieldByName('FTOTAL').Value),GmCentimeters);
                        fila:=fila+ClassImpresiones.SqlDatos4.FieldByName('FESPACI').Value;
                        SqlDatos4.next;
                    end;
                end;
                SqlDatos4.Active:=False;

                SqlDatos5.First;
                with FormPreview.VistaPrevia1.Canvas do begin
                    TextOut(19,24.2,FormatFloat('0',ClassImpresiones.SqlDatos5.FieldByName('FTOTAL').Value),GmCentimeters);
                    Font.Name:='Arial';
                    Font.Size:=7;
                    Font.Style:=[];
                    Textbox(6.1,24.7,20,27,ClassImpresiones.SqlDatos5.FieldByName('FTEXTO').Value,taLeftJustify,GmCentimeters);
                end;
                SqlDatos5.Active:=False;
            end;
            if sanciones=0 then  begin//va con saciones

                ClassImpresiones.SqlDatos6.Active:=False;
                ClassImpresiones.SqlDatos6.SQLs.SelectSQL.Text:='SELECT FECHA, CARPOR, FTEXTO FROM YYY_IMP_BOLETIN_SANCI_DETA('+#39+ClassImpresiones.SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(ClassImpresiones.SqlDatos.FieldByName('FCICLO').Value)+');';
                ClassImpresiones.SqlDatos6.Active:=True;
                if ClassImpresiones.SqlDatos6.RecordCount>0 then begin
                    FormPreview.VistaPrevia1.NewPage;
                    Fila:=2.3;
                    with FormPreview.VistaPrevia1.Canvas do begin
                       StretchDraw(0,0,20.5,29.5,ClassImpresiones.Imagen2,GmCentimeters);
                       Font.Name:='Arial';
                       Font.Size:=10;
                       Font.Style:=[];
                       With ClassImpresiones.SqlDatos Do begin
                          TextOut(9.5,Fila,FieldByName('FNOMBRE').Value,GmCentimeters);
                          fila:=fila+0.4;
                          TextOut(8,Fila,FieldByName('FDOCUMENTO').Value,GmCentimeters);
                          fila:=fila+0.5;
                          TextOut(5.8,Fila,IntToStr(FieldByName('FANIO').Value),GmCentimeters);
                          TextOut(8.8,Fila,FieldByName('FTURNO').Value,GmCentimeters);
                          TextOut(12.2,Fila,FieldByName('FDIVI').Value,GmCentimeters);
                          TextOut(19.2,Fila,IntToStr(FieldByName('FCICLO').Value),GmCentimeters);
                       end;
                    end;
                    fila:=5;
                    while not ClassImpresiones.SqlDatos6.Eof do begin
                         with FormPreview.VistaPrevia1.Canvas do begin
                           Font.Name:='Arial';
                           Font.Size:=11;
                           Font.Style:=[fsBold];
                           textosalida:='Fecha: '+FormatDateTime('dd/mm/yyyy',ClassImpresiones.SqlDatos6.FieldByName('FECHA').Value)+' Otorgada por: '+ClassImpresiones.SqlDatos6.FieldByName('CARPOR').AsString;
                           TextOut(0.9,Fila,textosalida,GmCentimeters);
                           fila:=fila+0.5;
                           Font.Size:=9;
                           Font.Style:=[];
                           textosalida:=ClassImpresiones.SqlDatos6.FieldByName('FTEXTO').Value;
                           TextoHeight:=TextBoxHeight(18,textosalida,GmCentimeters).AsCentimeters;
                           Textbox(1.5,fila,20,fila+TextoHeight,textosalida,taLeftJustify,GmCentimeters);
                           fila:=fila+TextoHeight;
                         end;
                         ClassImpresiones.SqlDatos6.next;
                    end;
                end;
                ClassImpresiones.SqlDatos6.Active:=False;
            end;
       ClassImpresiones.SqlDatos.Next;
       if not ClassImpresiones.SqlDatos.eof then
          FormPreview.VistaPrevia1.NewPage;
     end;
     formpreview.ShowModal;
  end;
  ClassImpresiones.Finaliza;
end;

Procedure Imp_Boletin_Inasistencias(carre, codalu, Vunidad, baja :String; anio, cutuco :Integer);
Var
   Fila:Real;
   textosalida:WideString;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=27;
  if ClassImpresiones.printdialog.Execute then begin
     ClassImpresiones.ImageJPG.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\boletin_inasistencias.jpg');
     FormPreview.VistaPrevia1.PaperSize:=A4;
     FormPreview.VistaPrevia1.Canvas.Pen.Width:=1;
     FormPreview.VistaPrevia1.Canvas.Pen.Color:=ClWhite;
     ClassImpresiones.SqlDatos.SQLs.SelectSQL.Text:='SELECT FCODALU, FANIO, FTURNO, FDIVI, FCICLO, FDOCUMENTO, FNOMBRE, FCUTUCO FROM YYY_IMP_BOLETIN_INA_ALUMNOS('+#39+carre+#39+','+#39+codalu+#39+','+IntToStr(anio)+','+#39+baja+#39+','+IntTostr(cutuco)+')'; //aca va el query que selecciona a los alumnos
     ClassImpresiones.IbTr.Active:=True;
     ClassImpresiones.SqlDatos.Active:=True;
     while not ClassImpresiones.SqlDatos.Eof do begin
        ClassImpresiones.SqlDatos2.Active:=False;
        ClassImpresiones.SqlDatos3.Active:=False;
        ClassImpresiones.SqlDatos2.SQLs.SelectSQL.Text:='SELECT FFECHACOL1, FDESCRICOL1, FCANTIDCOL1, FJUSTIFCOL1,FFECHACOL2, FDESCRICOL2, FCANTIDCOL2, FJUSTIFCOL2, FESPACI FROM YYY_IMP_BOLETIN_INA_DETALLE('+#39+ClassImpresiones.SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(ClassImpresiones.SqlDatos.FieldByName('FCICLO').Value)+');';
        ClassImpresiones.SqlDatos3.SQLs.SelectSQL.Text:='SELECT FTOTAL, FESPACI FROM YYY_IMP_BOLETIN_FALTAS('+#39+ClassImpresiones.SqlDatos.FieldByName('FCODALU').Value+#39+','+#39+carre+#39+','+IntToStr(ClassImpresiones.SqlDatos.FieldByName('FCICLO').Value)+');';
        ClassImpresiones.SqlDatos2.Active:=True;
        ClassImpresiones.SqlDatos3.Active:=True;
        ClassImpresiones.SqlDatos2.First;
        if (ClassImpresiones.SqlDatos2.RecordCount<>0) then begin
            Fila:=2.3;
            with FormPreview.VistaPrevia1.Canvas do begin
               StretchDraw(0.3,0.6,20.5,29.5,ClassImpresiones.ImageJPG.Graphic,GmCentimeters);
               Font.Name:='Arial';
               Font.Size:=10;
               Font.Style:=[];
               With ClassImpresiones.SqlDatos Do begin
                  TextOut(9.5,Fila,FieldByName('FNOMBRE').Value,GmCentimeters);
                  fila:=fila+0.4;
                  TextOut(8,Fila,FieldByName('FDOCUMENTO').Value,GmCentimeters);
                  fila:=fila+0.5;
                  TextOut(5.8,Fila,IntToStr(FieldByName('FANIO').Value),GmCentimeters);
                  TextOut(8.8,Fila,FieldByName('FTURNO').Value,GmCentimeters);
                  TextOut(12.2,Fila,FieldByName('FDIVI').Value,GmCentimeters);
                  TextOut(19.2,Fila,IntToStr(FieldByName('FCICLO').Value),GmCentimeters);
               end;
            end;
            with ClassImpresiones do begin
                fila:=6.05;
                with FormPreview.VistaPrevia1.Canvas do begin
                      Font.Name:='Arial';
                      Font.Size:=8;
                      Font.Style:=[];
                      while not SqlDatos2.Eof do begin
                          TextOut(0.95,Fila,ClassImpresiones.SqlDatos2.FieldByName('FFECHACOL1').Value,GmCentimeters);
                          TextOut(2.6,Fila,ClassImpresiones.SqlDatos2.FieldByName('FDESCRICOL1').Value,GmCentimeters);
                          TextOut(8,Fila,ClassImpresiones.SqlDatos2.FieldByName('FCANTIDCOL1').Value,GmCentimeters);
                          TextOut(9.3,Fila,ClassImpresiones.SqlDatos2.FieldByName('FJUSTIFCOL1').Value,GmCentimeters);

                          TextOut(10.8,Fila,ClassImpresiones.SqlDatos2.FieldByName('FFECHACOL2').Value,GmCentimeters);
                          TextOut(12.4,Fila,ClassImpresiones.SqlDatos2.FieldByName('FDESCRICOL2').Value,GmCentimeters);
                          TextOut(17.8,Fila,ClassImpresiones.SqlDatos2.FieldByName('FCANTIDCOL2').Value,GmCentimeters);
                          TextOut(19.1,Fila,ClassImpresiones.SqlDatos2.FieldByName('FJUSTIFCOL2').Value,GmCentimeters);

                          fila:=fila+ClassImpresiones.SqlDatos2.FieldByName('FESPACI').Value;
                          SqlDatos2.Next;
                      end;
                end;
                SqlDatos2.Active:=False;
                SqlDatos3.First;
                fila:=25.1;
                with FormPreview.VistaPrevia1.Canvas do begin
                    while not SqlDatos3.Eof do begin
                        TextOut(4.5,Fila,FormatFloat('0.00',ClassImpresiones.SqlDatos3.FieldByName('FTOTAL').Value),GmCentimeters);
                        fila:=fila+ClassImpresiones.SqlDatos3.FieldByName('FESPACI').Value;
                        SqlDatos3.next;
                    end;
                end;
                SqlDatos3.Active:=False;
            end;
            if not ClassImpresiones.SqlDatos.eof then
                FormPreview.VistaPrevia1.NewPage;
        end;
        ClassImpresiones.SqlDatos.Next;
        if ClassImpresiones.SqlDatos.eof then
           FormPreview.VistaPrevia1.DeleteCurrentPage;
     end;
     formpreview.ShowModal;
  end;
  ClassImpresiones.Finaliza;
end;


Procedure Imp_Calificador(const carre, codalu, Vunidad, baja:String;const anio, cutuco: integer; const estado: string);
Var
   Fila,TextoHeight :Real;
   textosalida:WideString;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=27;
  if ClassImpresiones.printdialog.Execute then begin
     ClassImpresiones.Imagen.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\calificador.wmf');
     FormPreview.VistaPrevia1.PaperSize:=A4;
     FormPreview.VistaPrevia1.Canvas.Pen.Width:=1;
     FormPreview.VistaPrevia1.Canvas.Pen.Color:=ClWhite;
     ClassImpresiones.SqlDatos.SQLs.SelectSQL.Text:='SELECT CODALU, ALUMNO, MATRIZ FROM YYY_IMP_CALIFICADOR_ALUMNOS('+IntTostr(cutuco)+','''+IntToStr(anio)+''','''+CARRE+''','''+estado+''')';
     ClassImpresiones.IbTr.Active:=True;
     ClassImpresiones.SqlDatos.Active:=True;
     ClassImpresiones.SqlDatos.first;
     fila:=4.3;
     FormPreview.VistaPrevia1.Canvas.StretchDraw(0,0,20.5,29.4,ClassImpresiones.Imagen,GmCentimeters);
     while not ClassImpresiones.SqlDatos.Eof do begin
            with FormPreview.VistaPrevia1.Canvas do begin
               Font.Name:='Arial';
               Font.Size:=10;
               Font.Style:=[];
               With ClassImpresiones.SqlDatos Do begin
                  TextOut(1   ,Fila,'Alumno: '+FieldByName('ALUMNO').Value,GmCentimeters);
                  TextOut(15.5,Fila,'Matriz: '+FieldByName('MATRIZ').Value,GmCentimeters);
               end;
            end;
            with ClassImpresiones do begin
                SqlDatos2.Active:=False;
                SqlDatos2.SQLs.SelectSQL.Text:='SELECT FINALTEXT, MATERIA, NOTA1, NOTA2, PROM, RECUP, FINAL, TEXTO, FESPACI FROM YYY_IMP_CALIFICADOR_MATERIAS('+IntTostr(cutuco)+','''+IntToStr(anio)+''','''+CARRE+''','''+ClassImpresiones.SqlDatos.FieldByName('CODALU').Value+''','''+estado+''')';
                SqlDatos2.Active:=True;
                SqlDatos2.First;
                fila:=fila+1.68;
                with FormPreview.VistaPrevia1.Canvas do begin
                      Font.Name:='Arial';
                      Font.Size:=9;
                      Font.Style:=[];
                      while not SqlDatos2.Eof do begin
                          TextOut(0.9,Fila,ClassImpresiones.SqlDatos2.FieldByName('MATERIA').Value,GmCentimeters);
                          if (ClassImpresiones.SqlDatos2.FieldByName('TEXTO').IsNull) then begin
                            if (not ClassImpresiones.SqlDatos2.FieldByName('NOTA1').IsNull) then
                                  TextOut(9,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('NOTA1').Value),GmCentimeters)
                            else if (ClassImpresiones.SqlDatos2.FieldByName('NOTA1').AsFloat=99) then
                                  TextOut(9,Fila,'Ausente',GmCentimeters);
                            if (not ClassImpresiones.SqlDatos2.FieldByName('NOTA2').IsNull) then
                                  TextOut(11,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('NOTA2').Value),GmCentimeters)
                            else if (ClassImpresiones.SqlDatos2.FieldByName('NOTA2').AsFloat=99) then
                                  TextOut(11,Fila,'Ausente',GmCentimeters);
                            if (not ClassImpresiones.SqlDatos2.FieldByName('PROM').IsNull) then
                                  TextOut(13.5,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('PROM').Value),GmCentimeters);
                            if (not ClassImpresiones.SqlDatos2.FieldByName('RECUP').IsNull) then
                                  TextOut(15.5,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('RECUP').Value),GmCentimeters)
                            else if (ClassImpresiones.SqlDatos2.FieldByName('RECUP').AsFloat=99) then
                                  TextOut(15.5,Fila,'Ausente',GmCentimeters);
                            if ClassImpresiones.SqlDatos2.FieldByName('FINALTEXT').IsNull then  begin
                              if (not ClassImpresiones.SqlDatos2.FieldByName('FINAL').IsNull) then
                                   TextOut(18.5,Fila,FormatFloat('0.00;;""',ClassImpresiones.SqlDatos2.FieldByName('FINAL').Value),GmCentimeters);
                            end
                            else
                              TextOut(17.7,Fila,ClassImpresiones.SqlDatos2.FieldByName('FINALTEXT').Value,GmCentimeters);
                          end
                          else
                            TextOut(8.5,Fila,ClassImpresiones.SqlDatos2.FieldByName('TEXTO').Value,GmCentimeters);
                          fila:=fila+ClassImpresiones.SqlDatos2.FieldByName('FESPACI').AsFloat;
                          SqlDatos2.Next;
                      end;
                end;
                SqlDatos2.Active:=False;
            end;
       fila:=fila+0.45;
       ClassImpresiones.SqlDatos.Next;
       if fila>=27 then begin
          FormPreview.VistaPrevia1.NewPage;
          FormPreview.VistaPrevia1.Canvas.StretchDraw(0,0,20.5,29.4,ClassImpresiones.Imagen,GmCentimeters);
          fila:=4.3;
       end;
     end;
     formpreview.ShowModal;
  end;
  ClassImpresiones.Finaliza;
end;


Procedure Imp_EtiqLegajo(const carre, codalu, Vunidad, baja:String;const anio, cutuco: integer);
Var
   Fila,TextoHeight :Real;
   textosalida:WideString;
   cont: integer;
   Tilde, Cruz : TMetaFile;
begin
  ClassImpresiones:=TClassImpresiones.Create;
  tilde := TMetaFile.Create;
  cruz := TMetaFile.Create;
  cruz.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\cruz.wmf');
  tilde.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\tilde.wmf');

  ClassImpresiones.Inicializa;
  ClassImpresiones.largopagina:=27;
  if ClassImpresiones.printdialog.Execute then begin
    ClassImpresiones.Imagen.LoadFromFile(vUnidad+CARPETA_PLANTILLA+'\legajo.wmf');
    FormPreview.VistaPrevia1.PaperSize:=LEGAL;
    FormPreview.VistaPrevia1.Orientation:=gmLandscape;
    FormPreview.VistaPrevia1.Canvas.Pen.Width:=1;
    FormPreview.VistaPrevia1.Canvas.Pen.Color:=ClWhite;
    ClassImpresiones.SqlDatos.SQLs.SelectSQL.Text:='SELECT NOMBRE,CARRE,COMISI,MATRIZ, DNI, '+
                                                   ' EQUIVAL, OBSERV, FDNI, XDNI, '+
                                                   ' FPARNAC, XPARNAC, CTT, XCTT, '+
                                                   ' TITSECU, XTITSECU, TITPRIM, XTITPRIM, '+
                                                   ' CPT, XCPT, PASE, XPASE, FOTO, XFOTO, APTFIS, XAPTFIS '+
                                                   'FROM YYY_IMP_LEGAJO('+IntTostr(cutuco)+','''+IntToStr(anio)+''','''+CARRE+''')';
    ClassImpresiones.IbTr.Active:=True;
    ClassImpresiones.SqlDatos.Active:=True;
    ClassImpresiones.SqlDatos.first;
    FormPreview.VistaPrevia1.Canvas.StretchDraw(0.5,0.7,35,20.7,ClassImpresiones.Imagen,GmCentimeters);
    cont:=0;
    fila:=0;
    while not ClassImpresiones.SqlDatos.Eof do begin
      with FormPreview.VistaPrevia1.Canvas do begin
        With ClassImpresiones.SqlDatos Do begin
          ClassImpresiones.SetFont('arial',30,[fsbold]);
          TextOut(1, 1+fila,FieldByName('NOMBRE').Value,GmCentimeters);
          ClassImpresiones.SetFont('arial',20,[fsbold]);
          TextOut(29.5, 0.8+fila,FieldByName('CARRE').Value,GmCentimeters);
          TextOut(29.5, 1.8+fila,FieldByName('COMISI').Value,GmCentimeters);
          TextOut(1, 2.9+fila,FieldByName('DNI').Value,GmCentimeters);
          TextOut(21.5, 2.9+fila,FieldByName('MATRIZ').Value,GmCentimeters);
          ClassImpresiones.SetFont('arial',15,[fsbold]);
          TextOut(1, 3.9+fila,FieldByName('EQUIVAL').Value,GmCentimeters);
          ClassImpresiones.SetFont('arial',12,[]);
          Textbox(0.8,4.5+fila,21,10.2+fila,FieldByName('OBSERV').Value,taLeftJustify,GmCentimeters);

          TextOut(22.5, 4+fila,FieldByName('FDNI').Value,GmCentimeters);
          TextOut(22.5, 4.7+fila,FieldByName('FPARNAC').Value,GmCentimeters);
          TextOut(22.5, 5.4+fila,FieldByName('CTT').Value,GmCentimeters);
          TextOut(22.5, 6.1+fila,FieldByName('TITSECU').Value,GmCentimeters);
          TextOut(22.5, 6.8+fila,FieldByName('TITPRIM').Value,GmCentimeters);
          TextOut(22.5, 7.5+fila,FieldByName('CPT').Value,GmCentimeters);
          TextOut(22.5, 8.2+fila  ,FieldByName('PASE').Value,GmCentimeters);
          TextOut(22.5, 8.9+fila,FieldByName('FOTO').Value,GmCentimeters);
          TextOut(22.5, 9.7+fila,FieldByName('APTFIS').Value,GmCentimeters);

          if AnsiSameText(FieldByName('XDNI').Value,'S') then
            StretchDraw(21.5,4+fila,22.2,4.6+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XDNI').Value,'N') then
            StretchDraw(21.5,4+fila,22.2,4.6+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XPARNAC').Value,'S') then
            StretchDraw(21.5,4.7+fila,22.2,5.3+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XPARNAC').Value,'N') then
            StretchDraw(21.5,4.7+fila,22.2,5.3+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XCTT').Value,'S') then
            StretchDraw(21.5,5.4+fila,22.2,6+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XCTT').Value,'N') then
            StretchDraw(21.5,5.4+fila,22.2,6+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XTITSECU').Value,'S') then
            StretchDraw(21.5,6.1+fila,22.2,6.7+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XTITSECU').Value,'N') then
            StretchDraw(21.5,6.1+fila,22.2,6.7+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XTITPRIM').Value,'S') then
            StretchDraw(21.5,6.8+fila,22.2,7.4+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XTITPRIM').Value,'N') then
            StretchDraw(21.5,6.8+fila,22.2,7.4+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XCPT').Value,'S') then
            StretchDraw(21.5,7.5+fila,22.2,8.1+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XCPT').Value,'N') then
            StretchDraw(21.5,7.5+fila,22.2,8.1+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XPASE').Value,'S') then
            StretchDraw(21.5,8.2+fila,22.2,8.8+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XPASE').Value,'N') then
            StretchDraw(21.5,8.2+fila,22.2,8.8+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XFOTO').Value,'S') then
            StretchDraw(21.5,8.9+fila,22.2,9.6+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XFOTO').Value,'N') then
            StretchDraw(21.5,8.9+fila,22.2,9.6+fila,cruz,GmCentimeters);
          if AnsiSameText(FieldByName('XAPTFIS').Value,'S') then
            StretchDraw(21.5,9.7+fila,22.2,10.4+fila,tilde,GmCentimeters)
          else if AnsiSameText(FieldByName('XAPTFIS').Value,'N') then
            StretchDraw(21.5,9.7+fila,22.2,10.4+fila,cruz,GmCentimeters);

        end;
        ClassImpresiones.SqlDatos.Next;
        cont:=cont+1;
        fila:=10.1;
        if (cont=2) and not ClassImpresiones.SqlDatos.Eof then begin
          FormPreview.VistaPrevia1.NewPage;
          FormPreview.VistaPrevia1.Canvas.StretchDraw(0.5,0.7,35,20.7,ClassImpresiones.Imagen,GmCentimeters);
          cont:=0;
          fila:=0;
        end;
      end;
    end;
    formpreview.ShowModal;
  end;
  tilde.Free;
  cruz.Free;
  ClassImpresiones.Finaliza;
end;

end.

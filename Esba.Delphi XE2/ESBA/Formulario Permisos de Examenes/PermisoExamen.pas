unit PermisoExamen;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Grids, DBGrids, StdCtrls, Buttons, DataModule, Mask, RXToolEdit,
  DB, IBCustomDataSet, IBQuery, IBDatabase, dbtables, FIBQuery, pFIBQuery,
  FIBDataSet, pFIBDataSet, FIBDatabase, pFIBDatabase, Variants,
  FuncionesConfiguracion, FuncionesDB, FuncionesExcel, FuncionesSystem, FuncionesText, FuncionesVariant, FuncionesPrint,
  Vcl.ExtCtrls, kbmMemTable;

type
  TPermisosExamen = class(TForm)
    GroupBox1: TGroupBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    MateriasCodigoAlumno: TEdit;
    MateriasCarreraCursa: TEdit;
    MateriasApellidoyNombre: TEdit;
    MateriasLibroMatriz: TEdit;
    DataSource1: TDataSource;
    GroupBox2: TGroupBox;
    Grilla: TDBGrid;
    NuevoPermiso: TBitBtn;
    modificaPermiso: TBitBtn;
    eliminaPermiso: TBitBtn;
    salir: TBitBtn;
    GroupBox3: TGroupBox;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    GrabaPermiso: TBitBtn;
    CancelaGrabacion: TBitBtn;
    NumeroPermiso: TEdit;
    Comision: TEdit;
    ComboMaterias: TComboEdit;
    NombreMateria: TLabel;
    ComboMesas: TComboEdit;
    Mesa: TLabel;
    TrPermisos: TpFIBTransaction;
    IBDelete: TpFIBQuery;
    IbupIns: TpFIBQuery;
    IBMaterias: TpFIBQuery;
    Panel1: TPanel;
    MTPermisos: TkbmMemTable;
    procedure NuevoPermisoClick(Sender: TObject);
    procedure CancelaGrabacionClick(Sender: TObject);
    procedure salirClick(Sender: TObject);
    procedure NumeroPermisoKeyPress(Sender: TObject; var Key: Char);
    procedure ComisionKeyPress(Sender: TObject; var Key: Char);
    procedure GrabaPermisoClick(Sender: TObject);
    procedure modificaPermisoClick(Sender: TObject);
    procedure eliminaPermisoClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure ComboMateriasButtonClick(Sender: TObject);
    procedure ComboMateriasExit(Sender: TObject);
    procedure ComboMesasButtonClick(Sender: TObject);
    procedure ComboMesasExit(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure FormShow(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  PermisosExamen: TPermisosExamen;
  VCarrera : String; //sirve para saber la carrera activa
  VUnidad : String; //sirve para saber la unidad en la cual se corre el sistema
  VCODALU : String; //pasa el codigo de alumno con el cual se esta trabajando
  VCondicion : String; // nose
  VApellido : String; //Pasa el apellido del alumno activo
  VNombre : String;  //Pasa el nombre del alumno activo
  VLibroMatriz : String; //Pasa el libro matriz del alumno
  VInstituto  : String; //Pasa el nomnbre del instituto
  VCaracteristica : String; //Pasa la caracteristica del colegio
  VCuatrimestreAnio : String; //guarda el año cuatrimestre actual
  VNombreUsuario : String; //Trae el usuario que esta trabajando
  Modificacion : Boolean; //sirve para saber si estoy modificando una materia o no
  Function ValidoMateria(Materias, Analitico : TQuery ;codigo:String):Boolean;
  Function CuentoLlamados(Cursada : TQuery;codigo:String):Boolean;
  Function VerificoSiEsta(Combo:TComboBox;Codigo:String;cont:Integer):Boolean;
  Function BuscoCutuco(Cursada : TQuery;Codigo : String):String;
implementation

{$R *.DFM}

Function BuscoCutuco(Cursada : TQuery;Codigo : String):String;
Begin
  Cursada.First;
  While (Not Cursada.Eof) And  (Cursada.FieldByName('COD_MAT').AsString <> Codigo) Do
    Cursada.Next;
  BuscoCutuco := IntToStr(Cursada.FieldByName('CUTUCO').AsInteger);
End;

Function VerificoSiEsta(Combo:TComboBox;Codigo:String;cont:Integer):Boolean;
Var
  x : Integer;
Begin
  VerificoSiEsta := False;
  For x:=0 To (Combo.Items.Count - 1) Do
    If codigo = Copy(Combo.Items.Strings[x],1,cont) Then
      Begin
        VerificoSiEsta := True;
        Combo.ItemIndex := x;
      End
End;


Function CuentoLlamados(Cursada : TQuery;codigo:String):Boolean;
Var
  CuatCursada, CuatLimite:String;
  Dia, mes, anio : Word;
Begin
  Cursada.First;
  While (Not Cursada.Eof) And (Cursada.FieldByName('COD_MAT').AsString <> Codigo) Do
    Cursada.Next;
  CuatCursada := Cursada.FieldByName('CUA_ANIO').AsString;
  If Copy(CuatCursada,1,1) = '1' Then
    CuatLimite := IntToStr(StrToInt(CuatCursada) + 3)
  Else
   If Copy(CuatCursada,1,1) = '2' Then
      CuatLimite := IntToStr(StrToInt(CuatCursada) -  97);
  If Copy(Cuatrimestre(vcarrera),2,2) > Copy(CuatLimite,2,2) Then
       CuentoLlamados := False
  Else
       CuentoLlamados := True;
  DecodeDate(Date(), anio, mes, dia);
  If CuatLimite = Cuatrimestre(vcarrera) Then
     If Mes >= 4 Then
        CuentoLlamados := False;
End;

Function ValidoMateria(Materias, Analitico : Tquery;codigo:String):Boolean;
Var
  Cont : Integer;
  CodMatVerificar:String;
  PuedeRendir, Mat23, Mat24:Boolean;
Begin
  Cont := 1;
  PuedeRendir := True;
  Mat23 := False;
  Mat24 := False;
  Materias.First;
  While (Not Materias.Eof) And  (Materias.FieldByName('CODMATERI').AsString <> Codigo) Do
    Materias.Next;
  If  Materias.FieldByName('CORRELATIV').AsString <> '' Then
    Begin
       While (cont<=15) And (PuedeRendir) do
         Begin
            CodMatVerificar := Copy(Materias.FieldByName('CORRELATIV').AsString,Cont,2);
              If CodMatVerificar <> '' Then
                Begin
                  Analitico.First;
                  While (Not Analitico.Eof) And (Analitico.FieldByName('COD_MAT').AsString <> CodMatVerificar) Do
                      Analitico.Next;
                  If Analitico.FieldByName('COD_MAT').AsString <> CodMatVerificar Then
                    Begin
                      PuedeRendir := False;
                      Cont := 17;
                    End;
                End;
            cont := cont + 3
         End;
    End;
  If (VCarrera = 'ASC') And (Codigo = '25') And (PuedeRendir) Then
   Begin
     Analitico.First;
     While (Not Analitico.Eof) Do
       Begin
         If (Analitico.FieldByName('COD_MAT').AsString = '24') Then
           Mat24 := True;
         If (Analitico.FieldByName('COD_MAT').AsString = '23') Then
           Mat23 := True;
        Analitico.Next;
       End;
     If Mat23 And Mat24 Then
        PuedeRendir := True
     Else
        PuedeRendir := False;
   End;
  ValidoMateria := PuedeRendir;
End;


procedure TPermisosExamen.NuevoPermisoClick(Sender: TObject);
begin
  PermisosExamen.Height := 523;
  NuevoPermiso.Enabled := False;
  ModificaPermiso.Enabled := False;
  EliminaPermiso.Enabled := False;
  Salir.Enabled :=  False;
  Grilla.Enabled := False;
  Modificacion := False;
  NumeroPermiso.Enabled := True;
  NumeroPermiso.SetFocus;
  //ComboMaterias.Enabled := True;
  Comision.Enabled := True;
  NumeroPermiso.Text := '';
  ComboMaterias.Text:='';
  ComboMesas.Text := '';
  Comision.Text := '';
end;

procedure TPermisosExamen.CancelaGrabacionClick(Sender: TObject);
begin
  PermisosExamen.Height := 345;
  NuevoPermiso.Enabled := True;
  ModificaPermiso.Enabled := True;
  EliminaPermiso.Enabled := True;
  Salir.Enabled :=  True;
  Grilla.Enabled := True;
  Modificacion := False;
  Grilla.SetFocus;
end;

procedure TPermisosExamen.salirClick(Sender: TObject);
begin
  Close;
  If TrPermisos.Active Then
     TrPermisos.Commit;
  //DsPermisos.Active:=False;
end;

procedure TPermisosExamen.NumeroPermisoKeyPress(Sender: TObject;
  var Key: Char);
begin
  If Not (charinset(key,['0'..'9',Chr(8), Chr(7), Chr(13), chr(9)])) Then
    Key := #0;
  If Key = #13 Then Begin
    comboMesas.SetFocus;
  End;
End;


procedure TPermisosExamen.ComisionKeyPress(Sender: TObject; var Key: Char);
begin
 If Not (charinset(key,['0'..'9',Chr(8), Chr(7), Chr(13), chr(9)])) Then
     Key := #0;
 If Key = #13 Then
   Begin
    GrabaPermiso.SetFocus;
   End;
end;


procedure TPermisosExamen.GrabaPermisoClick(Sender: TObject);
begin
  if (combomesas.Text='') or (combomaterias.Text='') or (comision.Text='') then begin
     MessageDlg( Pchar(VNombreUsuario + ', Faltan datos'),mtError,[mbok],0,mbok);
     exit;
  end;
  if MessageDlg(Pchar(VNombreUsuario + ', grabas el permiso de examen?'),mtConfirmation,[mbyes,mbno],0,mbno)=mryes  Then Begin
    try
      If Modificacion Then Begin
           IBupIns.SQL.Text:= 'UPDATE PERMEXA SET PERM_EXA=:PERM_EXA,MESA=:MESA,LLAMADO=:LLAMADO,APELLIDO=:APELLIDO,CUTUCO=:CUTUCO, '+
                              ' COD_MAT=:COD_MAT,FECH_EXA=:FECH_EXA,FECH_EMI=:FECH_EMI,NREG=:NREG,USUARIO=:USUARIO '+
                              ' WHERE COD_ALU=:COD_ALU AND CARRE=:CARRE AND MESA=:MESAORG AND COD_MAT=:COD_MAT_ORG ';
           IbupIns.ParamByName('MESAORG').AsInteger:= mtpermisos.fieldbyname('MESA').AsInteger;
           IbupIns.ParamByName('COD_MAT_ORG').AsString:= mtpermisos.fieldbyname('COD_MAT').AsString;
      End
      Else Begin
           IbupIns.SQL.Text:='INSERT INTO PERMEXA(PERM_EXA,MESA,LLAMADO,COD_ALU,APELLIDO,CUTUCO,' +
                             'CARRE,COD_MAT,FECH_EXA,FECH_EMI, NREG,USUARIO) '+
                             '             VALUES(:PERM_EXA,:MESA,:LLAMADO,:COD_ALU,:APELLIDO,' +
                             ':CUTUCO,:CARRE,:COD_MAT,:FECH_EXA,:FECH_EMI,:NREG,:USUARIO)';
      End;
      IbupIns.ParamByName('PERM_EXA').AsInteger := null;
      IbupIns.ParamByName('MESA').AsInteger      := null;
      IbupIns.ParamByName('COD_ALU').AsString    := null;
      IbupIns.ParamByName('APELLIDO').AsString   := null;
      IbupIns.ParamByName('CUTUCO').AsInteger    := null;
      IbupIns.ParamByName('CARRE').AsString      := null;
      IbupIns.ParamByName('COD_MAT').AsString    := null;
      IbupIns.ParamByName('FECH_EMI').AsDateTime := null;
      IbupIns.ParamByName('USUARIO').Value := null;
      if (NumeroPermiso.Text<>'') then
        IbupIns.ParamByName('PERM_EXA').AsInteger  := StrToInt(NumeroPermiso.Text);
      IbupIns.ParamByName('MESA').AsInteger      := StrToInt(ComboMesas.Text);
      IbupIns.ParamByName('COD_ALU').AsString    := VCodAlu;
      IbupIns.ParamByName('APELLIDO').AsString   := VApellido;
      IbupIns.ParamByName('CUTUCO').AsInteger    := StrToInt(Comision.Text);
      IbupIns.ParamByName('CARRE').AsString      := VCarrera;
      IbupIns.ParamByName('COD_MAT').AsString    := ComboMaterias.Text;
      IbupIns.ParamByName('FECH_EMI').AsDateTime := Date();
      IbupIns.ParamByName('USUARIO').Value := funcionesConfiguracion.CodUsu;

      TrPermisos.Active:= true;
      IbUpIns.ExecQuery;
      TrPermisos.Commit;
    finally
      //DsPermisos.Active:=False;
      //DsPermisos.Active:=True;
      TrPermisos.Active:=false;
      MtPermisos.Active:=false;
      FuncionesDB.Carga_MemTable(' SELECT P.PERM_EXA, M.SIGLA, P.MESA, P.LLAMADO, P.CUTUCO, P.FECH_EXA, P.FECH_EMI, P.COD_MAT, M.DESCRIPCI '+
                               ' FROM PERMEXA P '+
                               ' LEFT OUTER JOIN MATERIAS M ON P.COD_MAT=M.CODMATERI AND M.CODCARRE=P.CARRE '+
                               ' WHERE P.COD_ALU='''+VCODALU+''' AND P.CARRE='''+VCARRERA+'''', MtPermisos, true);

      If MtPermisos.RecordCount = 0 Then Begin
        ModificaPermiso.Enabled := False;
        EliminaPermiso.Enabled := False;
      End;
      CancelaGrabacionClick(Sender);
    end;
  End;
end;

procedure TPermisosExamen.modificaPermisoClick(Sender: TObject);
begin
 NuevoPermisoClick(Sender);
 NumeroPermiso.Text := IntToStr(MtPermisos.FieldByName('PERM_EXA').AsInteger);
 comboMaterias.text := MtPermisos.FieldByName('COD_MAT').AsString;
 NombreMateria.Caption:= MTPermisos.FieldByName('DESCRIPCI').AsString;
 comision.Text:= IntToStr(MTPermisos.FieldByName('CUTUCO').AsInteger);
 ComboMesas.Text := IntToStr(MTPermisos.FieldByName('MESA').AsInteger);
 Modificacion := True;
end;

procedure TPermisosExamen.eliminaPermisoClick(Sender: TObject);
begin
 if MessageDlg(Pchar(VNombreUsuario + ', esta segura/o de borrar el permiso Nº ' + IntToStr(MTPermisos.FieldByName('PERM_EXA').AsInteger)+'.'),mtConfirmation,[mbyes,mbno],0,mbno)=mryes  Then
    Begin
      IBDelete.ParamByName('COD_ALU').Value:=VCodAlu;
      IbDelete.ParamByName('COD_MAT').Value:=MTPermisos.fieldByName('COD_MAT').Value;
      IbDelete.ParamByName('CARRE').Value:=VCarrera;
      TrPermisos.Active:=True;
      IBDelete.ExecQuery;
      TrPermisos.Commit;
      //DsPermisos.Active:=False;
      //DsPermisos.Active:=True;
      //TrPermisos.Active:=True;
      MtPermisos.Active:=false;
      FuncionesDB.Carga_MemTable(' SELECT P.PERM_EXA, M.SIGLA, P.MESA, P.LLAMADO, P.CUTUCO, P.FECH_EXA, P.FECH_EMI, P.COD_MAT, M.DESCRIPCI '+
                                 ' FROM PERMEXA P '+
                                 ' LEFT OUTER JOIN MATERIAS M ON P.COD_MAT=M.CODMATERI AND M.CODCARRE=P.CARRE '+
                                 ' WHERE P.COD_ALU='''+VCODALU+''' AND P.CARRE='''+VCARRERA+'''', MtPermisos, true);

      If MtPermisos.RecordCount = 0 Then Begin
         ModificaPermiso.Enabled := False;
         EliminaPermiso.Enabled := False;
      End;
    End;
end;

procedure TPermisosExamen.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  If TrPermisos.Active Then
     TrPermisos.Commit;
  //DsPermisos.Active:=False;
end;

procedure TPermisosExamen.FormCreate(Sender: TObject);
begin
  TrPermisos.BeforeStart:=CustomerData.BaseBeforeStart;
end;

procedure TPermisosExamen.FormShow(Sender: TObject);
begin
  try
    MateriasCodigoAlumno.Text    := VCODALU;
    MateriasCarreraCursa.Text    := VCarrera;
    MateriasApellidoyNombre.Text := VApellido + ', ' + VNombre;
    MateriasLibroMatriz.Text     := Copy(VlibroMatriz,1,2) + '/' + copy(VlibroMatriz,3,3);
    PermisosExamen.Height := 345;
    //DsPermisos.ParamByName('COD_ALU').Value:=VCODALU;
    //DsPermisos.ParamByName('CARRE').Value:=VCARRERA;
    //DSPermisos.Active:=True;
    MtPermisos.Active:=false;
    FuncionesDB.Carga_MemTable(' SELECT P.PERM_EXA, M.SIGLA, P.MESA, P.LLAMADO, P.CUTUCO, P.FECH_EXA, P.FECH_EMI, P.COD_MAT, M.DESCRIPCI '+
                               ' FROM PERMEXA P '+
                               ' LEFT OUTER JOIN MATERIAS M ON P.COD_MAT=M.CODMATERI AND M.CODCARRE=P.CARRE '+
                               ' WHERE P.COD_ALU='''+VCODALU+''' AND P.CARRE='''+VCARRERA+'''', MtPermisos, true);

    If MtPermisos.RecordCount = 0 Then
     Begin
       ModificaPermiso.Enabled := False;
       EliminaPermiso.Enabled := False;
     End;
    TrPermisos.Active:=True;
    IBMaterias.ParamByName('COD_ALU').Value:=VCODALU;
    IBMaterias.ParamByName('CARRE').Value:=VCarrera;
    IBMaterias.ExecQuery;
    If (IBmaterias.RecordCount = 0) and (VCarrera<>'BAC') and (VCarrera<>'333') and (VCarrera<>'650') and (VCarrera<>'197916') Then
     Begin
       MessageDlg(Pchar(VNombreUsuario + ', al alumno no se le pueden cargar ningún permiso de exámen porque no posee ninguna materia REGULAR'),mtInformation,[mbok],0,mbok);
       NuevoPermiso.Enabled := False;
       ModificaPermiso.Enabled := False;
       EliminaPermiso.Enabled := False;
     End
  finally
    TrPermisos.commit;
    TrPermisos.Active:= false;
  end;
end;

procedure TPermisosExamen.ComboMateriasButtonClick(Sender: TObject);
Var b:Variant;
begin
  FuncionesDB.combobusqueda('SELECT M.MESA, M.LLAMADO, M.FECH_EXA, M.HORA, T.DESCRI, X.CODMAT, X.MATERIA, X.CONDICION, X.FERRCOD, X.FERRMSG '+
                            'FROM XXX_MATERIAS_FINALES('+#39+VCODALU+#39+','+#39+VCARRERA+#39+') X'+
                            'LEFT OUTER JOIN MESAS M ON  M.CARRE='''+VCARRERA+''' AND M.COD_MAT= X.CODMAT '+
                            'LEFT OUTER JOIN MESA_TIPO T ON T.CODIGO=M.TIPMES ',['Codigo','Nombre','Condicion'],b,True);
  ComboMaterias.Text:=b[0];
  NombreMateria.Caption:=b[1];
end;

procedure TPermisosExamen.ComboMateriasExit(Sender: TObject);
Var b:Variant;
begin
  if ((ComboMaterias.Text<>'') and (ComboMaterias.Text <> MtPermisos.FieldByName('COD_MAT').AsString)) Then Begin
      ComBoMaterias.Text:=Lpad(ComboMaterias.Text,2,'0');
      FuncionesDB.combobusqueda('SELECT CUTUCO, MATERIA, FERRCOD, FERRMSG FROM XXX_MATERIAS_FINALES('+#39+VCODALU+#39+','+#39+VCARRERA+#39+') WHERE CODMAT='+#39+ComBoMaterias.Text+#39,['Codigo','Nombre'],b,False);
      If (vartostr(b[0])='') then begin
         MessageDlg('Código incorrecto',mtError,[mbok],0,mbok);
         ComboMaterias.SetFocus;
      end
      else if (b[2]=2) then begin
         MessageDlg(Pchar(VNombreUsuario + vartostr(b[3])),mtInformation,[mbok],0,mbok);
         ComboMaterias.SetFocus;
      end
      else if (b[2]=1) then begin
         if MessageDlg(Pchar(VNombreUsuario + vartostr(b[3])),mtConfirmation,[mbyes,mbno],0,mbno)=mrno then
            ComboMaterias.SetFocus
         else begin
            comision.Text:=b[0];
            NombreMateria.Caption:=b[1];
         end;
      end
      else if (b[2]=0) then begin
          comision.Text:=b[0];
          NombreMateria.Caption:=b[1];
      end
  end

end;

procedure TPermisosExamen.ComboMesasButtonClick(Sender: TObject);
Var b:Variant;
begin
  FuncionesDB.combobusqueda('SELECT M.MESA, M.LLAMADO, M.FECH_EXA, M.HORA, X.CODMAT, X.MATERIA, X.CONDICION, X.CUTUCO,T.DESCRI, X.FERRCOD, X.FERRMSG '+
                            'FROM XXX_MATERIAS_FINALES('+#39+VCODALU+#39+','+#39+VCARRERA+#39+') X '+
                            'LEFT OUTER JOIN MESAS M ON  M.CARRE='''+VCARRERA+''' AND M.COD_MAT= X.CODMAT '+
                            'LEFT OUTER JOIN MESA_TIPO T ON T.CODIGO=M.TIPMES ',['Mesa','Llamado','Fecha Exa.','Hora','Codigo','Nombre','Condicion'],b,True);
  ComboMesas.Text:=b[0];
  NombreMateria.Caption:=b[5];
  ComboMaterias.Text:= b[4];
  Comision.Text:= b[7];
end;

procedure TPermisosExamen.ComboMesasExit(Sender: TObject);
Var b:variant;
begin
  if (ComboMesas.Text<>'') Then Begin
      FuncionesDB.combobusqueda('SELECT M.MESA, M.LLAMADO, M.FECH_EXA, M.HORA, X.CODMAT, X.MATERIA, X.CONDICION, X.CUTUCO, T.DESCRI, X.FERRCOD, X.FERRMSG '+
                            'FROM XXX_MATERIAS_FINALES('+#39+VCODALU+#39+','+#39+VCARRERA+#39+') X '+
                            'LEFT OUTER JOIN MESAS M ON  M.CARRE='''+VCARRERA+''' AND M.COD_MAT= X.CODMAT '+
                            'LEFT OUTER JOIN MESA_TIPO T ON T.CODIGO=M.TIPMES '+
                            'where M.MESA='+ComboMesas.Text,['Mesa','Llamado','Fecha Exa.','Hora','Codigo','Nombre','Condicion'],b,False);
      If (vartostr(b[0])='') then begin
         MessageDlg('Código incorrecto',mtError,[mbok],0,mbok);
         Comision.SetFocus;
      end
      else if (b[9]=2) then begin
         MessageDlg(Pchar(VNombreUsuario + vartostr(b[10])),mtInformation,[mbok],0,mbok);
         ComboMesas.SetFocus;
      end
      else if (b[9]=1) then begin
         if MessageDlg(Pchar(VNombreUsuario + vartostr(b[10])),mtConfirmation,[mbyes,mbno],0,mbno)=mrno then
            ComboMesas.SetFocus
         else begin
            ComboMesas.Text:=b[0];
            NombreMateria.Caption:=b[5];
            ComboMaterias.Text:= b[4];
            Comision.Text:= b[7];
         end;
      end
      else begin
        ComboMesas.Text:=b[0];
        NombreMateria.Caption:=b[5];
        ComboMaterias.Text:= b[4];
        Comision.Text:= b[7];
      end;
  end
end;

end.

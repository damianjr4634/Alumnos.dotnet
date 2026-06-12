unit FrmEsba;

interface

uses
  RXCurrEdit,Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Menus, ExtCtrls, StdCtrls, ImgList,
  ComCtrls, Db, Grids, DBGrids, DBCtrls, BDE, ShellApi, inifiles,
  Buttons, system.StrUtils, System.DateUtils,
  Mask, RXToolEdit,  pngimage, GIFImg, jpeg, System.Variants,
//MODULOS DE PRUEBA
  UTHRead, AppEvnts,
  //dxSkinsdxBarPainter,
  cxGraphics, cxControls,
  //cxLookAndFeels,
  //cxLookAndFeelPainters,
  //dxSkinsdxNavBarPainter,
  dxNavBarCollns,
  //dxNavBarBase,
  //dxNavBar,
  dxBar,
  dxBarExtItems,
  cxClasses, kbmMemTable,
  RXDBCtrl, dxSkinsCore, dxSkinsdxBarPainter; //dxSkinsCore, dxSkinBlack, dxSkinBlue,
  //dxSkinBlueprint, dxSkinCaramel, dxSkinCoffee, dxSkinDarkRoom, dxSkinDarkSide,
  //dxSkinDevExpressDarkStyle, dxSkinDevExpressStyle, dxSkinFoggy,
  //dxSkinGlassOceans, dxSkinHighContrast, dxSkiniMaginary, dxSkinLilian,
  //dxSkinLiquidSky, dxSkinLondonLiquidSky, dxSkinMcSkin, dxSkinMoneyTwins,
  //dxSkinOffice2007Black, dxSkinOffice2007Blue, dxSkinOffice2007Green,
  //dxSkinOffice2007Pink, dxSkinOffice2007Silver, dxSkinOffice2010Black,
  //dxSkinOffice2010Blue, dxSkinOffice2010Silver, dxSkinPumpkin, dxSkinSeven,
  //dxSkinSevenClassic, dxSkinSharp, dxSkinSharpPlus, dxSkinSilver,
  //dxSkinSpringTime, dxSkinStardust, dxSkinSummer2008, dxSkinTheAsphaltWorld,
  //dxSkinsDefaultPainters, dxSkinValentine, dxSkinVS2010, dxSkinWhiteprint,
  //dxSkinXmas2008Blue;

type
  TFormEsba = class(TForm)
    dxBarDockControl1: TdxBarDockControl;
    BarradeEstado: TStatusBar;
    ImagenesCarreras: TImageList;
    MnuManager: TdxBarManager;
    DocentesporComision1: TdxBarButton;
    Mesasdeexamen1: TdxBarButton;
    Carrera1: TdxBarSubItem;
    MateriasporComision1: TdxBarButton;
    FinalespormesasyComision1: TdxBarButton;
    Regularizacion1: TdxBarSubItem;
    Credenciales1: TdxBarButton;
    MorososporFecha1: TdxBarButton;
    DocumentacionporComision1: TdxBarButton;
    DocumentacionGeneral1: TdxBarButton;
    ExtranjerosporComision1: TdxBarButton;
    Localidad1: TdxBarButton;
    EstadoCivil1: TdxBarButton;
    FechadeIngreso1: TdxBarButton;
    Sexo1: TdxBarButton;
    Edad1: TdxBarButton;
    CodigoPostal1: TdxBarButton;
    Mac1: TdxBarButton;
    Morosos1: TdxBarButton;
    EstadisticasdeAlumnos1: TdxBarSubItem;
    ListadoGeneral1: TdxBarSubItem;
    AsignaturaporEquivalencia1: TdxBarButton;
    EquivalenciasInternas1: TdxBarButton;
    ListadodeEquivalencias1: TdxBarSubItem;
    InscriptosporComisionycuatrimestre1: TdxBarButton;
    Completoporcuatrimestre1: TdxBarButton;
    Nuevos1: TdxBarButton;
    ListadodeInscriptos1: TdxBarSubItem;
    ReincorporacionporComMatCuat1: TdxBarButton;
    LibresporComMatCuat1: TdxBarButton;
    RecursantecompletoporCuat1: TdxBarButton;
    PorCuatMatCondi1: TdxBarButton;
    PorCuatCondicionCompleto1: TdxBarButton;
    Queadeudenmasde2materiascompleto1: TdxBarButton;
    Egresadosenrangodefechas1: TdxBarButton;
    ProbablesegresadosporCuat1: TdxBarButton;
    ListadossegunCondicion1: TdxBarSubItem;
    Alumnos2: TdxBarSubItem;
    Comisionesarmadas1: TdxBarButton;
    Carpetaasistencia1: TdxBarButton;
    Asistenciaprovisoria1: TdxBarButton;
    Carpetadetrabajos1: TdxBarButton;
    Planillasdeprofesores1: TdxBarButton;
    Profesores1: TdxBarSubItem;
    Volantes1: TdxBarButton;
    Parciales1: TdxBarButton;
    Reincorporacion1: TdxBarButton;
    ARegular1: TdxBarButton;
    Actoas1: TdxBarSubItem;
    Profesoresporcomision1: TdxBarButton;
    Comisionesalmnisterio1: TdxBarButton;
    Titulosylegalizaciones1: TdxBarButton;
    Ministerio1: TdxBarSubItem;
    Listados1: TdxBarSubItem;
    Alumno: TdxBarSubItem;
    Nuevo: TdxBarButton;
    Busqueda: TdxBarSubItem;
    Grilla: TDBGrid;
    DataSource1: TDataSource;
    Timer1: TTimer;
    Materias: TdxBarSubItem;
    dxBarButton1: TdxBarButton;
    dxBarButton2: TdxBarButton;
    ImagenesBarra: TImageList;
    dxBarButton3: TdxBarButton;
    dxBarButton4: TdxBarButton;
    dxBarButton5: TdxBarButton;
    dxBarButton6: TdxBarButton;
    dxBarButton7: TdxBarButton;
    dxBarButton8: TdxBarButton;
    dxBarButton9: TdxBarButton;
    dxBarButton10: TdxBarButton;
    dxBarButton12: TdxBarButton;
    dxBarSubItem1: TdxBarSubItem;
    dxBarSubItem2: TdxBarSubItem;
    dxBarSubItem3: TdxBarSubItem;
    dxBarSubItem4: TdxBarSubItem;
    dxBarButton13: TdxBarButton;
    dxBarButton14: TdxBarButton;
    NotasUsuario: TRichEdit;
    Panel1: TPanel;
    SpeedButton1: TSpeedButton;
    SpeedButton2: TSpeedButton;
    SpeedButton3: TSpeedButton;
    SpeedButton4: TSpeedButton;
    Contextual: TdxBarPopupMenu;
    ModificarAlumno: TdxBarButton;
    Salir: TdxBarButton;
    dxBarButton11: TdxBarButton;
    dxBarButton15: TdxBarButton;
    dxBarSubItem5: TdxBarSubItem;
    MnuContexFuente: TdxBarPopupMenu;
    MnuFuente: TdxBarFontNameCombo;
    MnuColorFuente: TdxBarColorCombo;
    MnuTamanio: TdxBarCombo;
    MnuEstilo: TdxBarSubItem;
    MnuNegrita: TdxBarButton;
    MnuCursiva: TdxBarButton;
    MnuSubrrayado: TdxBarButton;
    dxBarSubItem6: TdxBarSubItem;
    MnuColorFondo: TdxBarColorCombo;
    PanelBusqueda: TPanel;
    dxBarSubItem7: TdxBarSubItem;
    NoInscriptos: TdxBarButton;
    MnuListados: TdxBarButton;
    dxBarButton16: TdxBarButton;
    dxBarSubItem8: TdxBarSubItem;
    ApplicationEvents1: TApplicationEvents;
    dxBarButton17: TdxBarButton;
    FechaMemo: TDateEdit;
    dxBarButton18: TdxBarButton;
    dxBarButton19: TdxBarButton;
    dxBarSubItem9: TdxBarSubItem;
    dxBarSubItem10: TdxBarSubItem;
    ConsultadeFaltascomision: TdxBarButton;
    ConsultadeFaltasalumno: TdxBarButton;
    dxBarSubItem11: TdxBarSubItem;
    dxBarButton20: TdxBarButton;
    dxBarButton21: TdxBarButton;
    dxBarButton22: TdxBarButton;
    dxBarSubItem12: TdxBarSubItem;
    dxBarSubItem13: TdxBarSubItem;
    dxBarButton23: TdxBarButton;
    dxBarSubItem14: TdxBarSubItem;
    dxBarButton24: TdxBarButton;
    dxBarSubItem15: TdxBarSubItem;
    dxBarButton25: TdxBarButton;
    Panel3: TPanel;
    MtAlumnos: TkbmMemTable;
    MtAlumnoscod_alu: TStringField;
    MtAlumnosMATRIZ: TStringField;
    MtAlumnosAPELLIDO: TStringField;
    MtAlumnosNOM_APE: TStringField;
    MtAlumnosMAIL: TStringField;
    MtAlumnosFOTO: TBlobField;
    MtAlumnosDATOS: TStringField;
    dbMemo: TDBMemo;
    Imagen: TImage;
    Image1: TImage;
    MtAlumnosFMENSAJE: TStringField;
    MtAlumnosFCOLOR: TIntegerField;
    DbObserv: TDBMemo;
    MtAlumnosOBSERV: TMemoField;
    Splitter2: TSplitter;
    Splitter3: TSplitter;
    Splitter4: TSplitter;
    dxBarButton26: TdxBarButton;
    dxBarButton27: TdxBarButton;
    dxBarButton28: TdxBarButton;
    dxBarButton29: TdxBarButton;
    dxBarButton30: TdxBarButton;
    dxBarSubItem16: TdxBarSubItem;
    dxBarButton31: TdxBarButton;
    dxBarButton32: TdxBarButton;
    dxBarButton33: TdxBarButton;
    dxBarButton34: TdxBarButton;
    dxBarButton35: TdxBarButton;
    dxBarButton36: TdxBarButton;
    dxBarButton37: TdxBarButton;
    Label2: TLabel;
    TxtBusqueda: TEdit;
    dxBarButton38: TdxBarButton;
    MtAlumnosCARRERA: TStringField;
    chbBuscarBajas: TCheckBox;
    VerNotas: TSpeedButton;
    MtAlumnosRESOLUCION: TStringField;
    MtAlumnosCODCARRE: TStringField;
    MtAlumnosBAJA: TStringField;
    dxBarButton39: TdxBarButton;
    dxBarSubItem17: TdxBarSubItem;
    dxBarButton40: TdxBarButton;
    dxBarSubItem18: TdxBarSubItem;
    MnuAltaUsuario: TdxBarButton;
    mnuBajaUsuarios: TdxBarButton;
    mnucambiopass: TdxBarButton;
    mnupermisos: TdxBarButton;
    mnuresetpass: TdxBarButton;
    dxBarSubItem19: TdxBarSubItem;
    dxBarSubItem20: TdxBarSubItem;
    mnudocentes: TdxBarButton;
    dxBarSubItem21: TdxBarSubItem;
    mnuListadoDocentes: TdxBarButton;
    mnucercerv: TdxBarButton;
    Panel2: TPanel;
    BtnBuscar: TBitBtn;
    MtAlumnosMODALIDAD: TStringField;
    Label1: TLabel;
    cbCarreDesuso: TCheckBox;
    MtAlumnosCARRERASC: TStringField;
    Panel4: TPanel;
    GroupBox1: TGroupBox;
    LblApellidoNombre: TLabel;
    LblCarrera: TLabel;
    dxBarSubItem22: TdxBarSubItem;
    dxBarButton41: TdxBarButton;
    dxBarButton42: TdxBarButton;
    dxBarTablaDeConfiguraciones: TdxBarButton;
    dxBarButton43: TdxBarButton;
    dxBarButton44: TdxBarButton;
    dxBarSubItem23: TdxBarSubItem;
    dxBarButton45: TdxBarButton;
    procedure NuevoClick(Sender: TObject);
    procedure Timer1Timer(Sender: TObject);
    procedure dxBarButton1Click(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure SpeedButton4Click(Sender: TObject);
    procedure SpeedButton1Click(Sender: TObject);
    procedure SpeedButton2Click(Sender: TObject);
    procedure SpeedButton3Click(Sender: TObject);
    procedure FechaMemoCloseUp(Sender: TObject);
    procedure dxBarButton8Click(Sender: TObject);
    procedure dxBarButton9Click(Sender: TObject);
    procedure GrillaMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure ModificarAlumnoClick(Sender: TObject);
    procedure GrillaDblClick(Sender: TObject);
    procedure GrillaKeyPress(Sender: TObject; var Key: Char);
    procedure SalirClick(Sender: TObject);
    procedure dxBarButton12Click(Sender: TObject);
    procedure Mesasdeexamen1Click(Sender: TObject);
    procedure MateriasporComision1Click(Sender: TObject);
    procedure dxBarButton2Click(Sender: TObject);
    procedure dxBarButton3Click(Sender: TObject);
    procedure dxBarButton4Click(Sender: TObject);
    procedure dxBarButton5Click(Sender: TObject);
    procedure dxBarButton13Click(Sender: TObject);
    procedure dxBarButton14Click(Sender: TObject);
    procedure dxBarButton10Click(Sender: TObject);
    procedure dxBarSubItem5Popup(Sender: TObject);
    procedure dxBarButton11Click(Sender: TObject);
    procedure ContextualPopup(Sender: TObject);
    procedure DocentesporComision1Click(Sender: TObject);
    procedure dxBarButton15Click(Sender: TObject);
    procedure FinalespormesasyComision1Click(Sender: TObject);
    procedure MnuFuenteClick(Sender: TObject);
    procedure NotasUsuarioMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure FechaMemoChange(Sender: TObject);
    procedure GrillaKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure GrillaDrawColumnCell(Sender: TObject; const Rect: TRect;
      DataCol: Integer; Column: TColumn; State: TGridDrawState);
    procedure VerNotasClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure Carpetaasistencia1Click(Sender: TObject);
    procedure Parciales1Click(Sender: TObject);
    procedure Planillasdeprofesores1Click(Sender: TObject);
    procedure Carpetadetrabajos1Click(Sender: TObject);
    procedure Volantes1Click(Sender: TObject);
    procedure Reincorporacion1Click(Sender: TObject);
    procedure ARegular1Click(Sender: TObject);
    procedure ApplicationEvents1Message(var Msg: tagMSG;
      var Handled: Boolean);
    procedure dxBarButton17Click(Sender: TObject);
    procedure dxBarButton7Click(Sender: TObject);
    procedure FormKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure Comisionesalmnisterio1Click(Sender: TObject);
    procedure dxBarButton18Click(Sender: TObject);
    procedure dxBarButton19Click(Sender: TObject);
    procedure dxBarButton20Click(Sender: TObject);
    procedure dxBarButton21Click(Sender: TObject);
    procedure dxBarButton22Click(Sender: TObject);
    procedure dxBarButton23Click(Sender: TObject);
    procedure dxBarButton24Click(Sender: TObject);
    procedure dxBarButton25Click(Sender: TObject);
    procedure MtAlumnosAfterScroll(DataSet: TDataSet);
    procedure dxBarButton26Click(Sender: TObject);
    procedure dxBarButton27Click(Sender: TObject);
    procedure dxBarButton28Click(Sender: TObject);
    procedure dxBarButton29Click(Sender: TObject);
    procedure dxBarButton30Click(Sender: TObject);
    procedure dxBarButton31Click(Sender: TObject);
    procedure dxBarButton32Click(Sender: TObject);
    procedure dxBarButton34Click(Sender: TObject);
    procedure dxBarButton37Click(Sender: TObject);
    procedure BtnBajasAltasClick(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure BtnBuscarClick(Sender: TObject);
    procedure TxtBusquedaKeyPress(Sender: TObject; var Key: Char);
    procedure MnuAltaUsuarioClick(Sender: TObject);
    procedure mnuBajaUsuariosClick(Sender: TObject);
    procedure mnucambiopassClick(Sender: TObject);
    procedure mnupermisosClick(Sender: TObject);
    procedure mnuresetpassClick(Sender: TObject);
    procedure mnudocentesClick(Sender: TObject);
    procedure mnucercervClick(Sender: TObject);
    procedure mnuListadoDocentesClick(Sender: TObject);
    procedure dxBarButton41Click(Sender: TObject);
    procedure dxBarButton42Click(Sender: TObject);
    procedure dxBarTablaDeConfiguracionesClick(Sender: TObject);
    procedure GrillaTitleClick(Column: TColumn);
    procedure dxBarButton43Click(Sender: TObject);
    procedure dxBarButton45Click(Sender: TObject);
  private
    { Private declarations }
    FCodAlu: string;
    //FBajas : String;
    FThAlumnos : TFISQLThread;
    FrmProgress: TForm;
    FBarra : TProgressBar;
    Procedure AbroBDDAlumnosTHRead(const ACodAlu:String='');
    procedure ThOnterminated(sender: TObject);
    function SeleccionarCarrera(const aDesact:string):string; overload;
    function SeleccionarCarrera(const aDesact:string; var aDesCarre:string):string; overload;
  public
    { Public declarations }
    Procedure CambioFuente(Fuente:Tfont);
    Procedure Finalizacion;
    Procedure CargoConfiguracion(Seccion : String);
    Procedure ListadoDinamico(Sender: TObject);
    Procedure Ultima_seleccion(Sender:TObject; opc:Integer);
    Procedure Reseteo_pass;
  end;

Type TUltSel=Record
  Objeto:Tobject;
  Param:Variant;
end;

var
  FormEsba: TFormEsba;
  UnidadPrograma : String;
  PathListados: string;
  Nombreusuario : String;
  //NombreCarrera : String;
  MenuContextual : String;
  Pos :Integer;
  UltSel:TUltSel;


implementation


{$R *.DFM}

uses
   FuncionesConfiguracion, FuncionesText, FuncionesDB, Carreras, NotasExamenFinal, Equivalencia, CambioCodAlu_LM,ModifAnalitico,
  cargacomisiones,CertServ,AltaUsuario,enviocorreo,FinalesxMesayComision,
  BajaUsuarios, DataModule, modulovariable,lstplanasis, lstactasexamenes,
  lstNotasyPractico,lstactasreincorporacion,lstactasARegular, lstactasMesas,
  lst_impresion_equivalencia_terc, lst_impresion_equivalencia_bac,
  ComisionesAlMinisterio, Tutores, CargadePermisosMasivo,
  CambioPassword, Parametros,CargaInasistenciasComision, impresiones,
  CargadeTrimestres, ConsultaReincorporaciones, Seciones, ModificacionInasistenciasComision,
  CargaInasistenciasComisionNuevo,RegularizacionDeMaterias_nuevo,
  RegularizacionDeMateriasXComision_nuevo,
  ConstanciaAlumnos2, PermisosPorUsuario, FrmAltaAlumno, sesion, InscripcionDeMaterias,
  constanciaalumnos, constanciaalumnoregular, FrmAltaModProfes,
  ElimProfes, altamodifmaterias, MesasExamen,
  PermisoExamen, CargadePermisosWeb, InscripcionesWeb, TablaConfiguraciones;

Const
   Panel_Institucion = 1;
   Panel_Registros = 2;
   Panel_Usuario = 3;
   Panel_Dia     = 4;
   Panel_Hora    = 5;
   Panel_Path = 0;

   //****FUNCIONES DE CADA CARRERA*****//

Procedure TFormEsba.Ultima_seleccion(Sender:TObject; opc:Integer);
begin
   if (UltSel.Objeto<>Sender) and (opc=1) then
      VarClear(Parametros.UltParam);
   UltSel.Objeto:=Sender;
   if (UltSel.Objeto is TdxBarButton) and (opc=2) then
      TdxBarButton(UltSel.Objeto).OnClick(UltSel.Objeto);
end;

Procedure TFormEsba.CargoConfiguracion(Seccion : String);
Var
  IniFile :TiniFile;
Begin
     IniFile := TIniFile.Create(UnidadPrograma+'\Esba_prg.ini');

     Grilla.Font.Name := IniFile.ReadString(Seccion,'NombreFuenteGrilla',Grilla.Font.Name);
     Grilla.Font.Size := IniFile.ReadInteger(Seccion,'TamanioFuenteGrilla', Grilla.Font.Size);
     Grilla.Font.Color := StringToColor(IniFile.ReadString(Seccion,'ColorFuenteGrilla',ColorToString(Grilla.Font.Color)));
     If IniFile.ReadBool(Seccion,'NegritaFuenteGrilla',FsBold in Grilla.Font.Style) Then
       Grilla.Font.Style := Grilla.Font.Style + [FsBold]
     Else
       Grilla.Font.Style := Grilla.Font.Style - [FsBold];
     If  IniFile.ReadBool(Seccion,'CursivaFuenteGrilla',FsItalic in Grilla.Font.Style) Then
         Grilla.Font.Style := Grilla.Font.Style + [FsItalic]
     Else
       Grilla.Font.Style := Grilla.Font.Style - [FsItalic];
     If  IniFile.ReadBool(Seccion,'SubrrayadoFuenteGrilla',Fsunderline in Grilla.Font.Style) Then
         Grilla.Font.Style := Grilla.Font.Style + [FsUnderline]
     Else
       Grilla.Font.Style := Grilla.Font.Style - [FsUnderline];
     Grilla.Color := StringToColor(IniFile.ReadString(Seccion,'ColorFondoGrilla',ColorToString(Grilla.Color)));

     NotasUsuario.Font.Name := IniFile.ReadString(Seccion,'NombreFuenteMemo',NotasUsuario.Font.Name);
     NotasUsuario.Font.Size := IniFile.ReadInteger(Seccion,'TamanioFuenteMemo', NotasUsuario.Font.Size);
     NotasUsuario.Font.Color := StringToColor(IniFile.ReadString(Seccion,'ColorFuenteMemo',ColorToString(NotasUsuario.Font.Color)));
     If IniFile.ReadBool(Seccion,'NegritaFuenteMemo',FsBold in NotasUsuario.Font.Style) Then
       NotasUsuario.Font.Style := NotasUsuario.Font.Style + [FsBold]
     Else
       NotasUsuario.Font.Style := NotasUsuario.Font.Style - [FsBold];
     If IniFile.ReadBool(Seccion,'CursivaFuenteMemo',FsItalic in NotasUsuario.Font.Style) Then
       NotasUsuario.Font.Style := NotasUsuario.Font.Style + [FsItalic]
     Else
       NotasUsuario.Font.Style := NotasUsuario.Font.Style - [FsItalic];
     If IniFile.ReadBool(Seccion,'SubrrayadoFuenteMemo',Fsunderline in NotasUsuario.Font.Style) Then
       NotasUsuario.Font.Style := NotasUsuario.Font.Style + [FsUnderline]
     Else
       NotasUsuario.Font.Style := NotasUsuario.Font.Style - [FsUnderline];
     NotasUsuario.Color := StringToColor(IniFile.ReadString(Seccion,'ColorFondoMemo',ColorToString(NotasUsuario.Color)));

     IniFile.Free;
     Grilla.Refresh;
     NotasUsuario.Refresh;
End;


Procedure TFormEsba.Finalizacion;
Var
  IniFile : TIniFile;
begin
  MnuManager.SaveToIniFile(UnidadPrograma+CARPETA_NOTAS+'\'+NombreUsuario+'.bar');
  NotasUsuario.Lines.SaveToFile(UnidadPrograma+CARPETA_NOTAS+'\'+NombreUsuario+'.not');

  IniFile := TIniFile.Create(UnidadPrograma+'\Esba_prg.ini');

  IniFile.WriteString(NombreUsuario,'NombreFuenteGrilla',Grilla.Font.Name);
  IniFile.WriteInteger(NombreUsuario,'TamanioFuenteGrilla', Grilla.Font.Size);
  IniFile.WriteString(NombreUsuario,'ColorFuenteGrilla',ColorToString(Grilla.Font.Color));
  IniFile.WriteBool(NombreUsuario,'NegritaFuenteGrilla',FsBold in Grilla.Font.Style);
  IniFile.WriteBool(NombreUsuario,'CursivaFuenteGrilla',FsItalic in Grilla.Font.Style);
  IniFile.WriteBool(NombreUsuario,'SubrrayadoFuenteGrilla',Fsunderline in Grilla.Font.Style);
  IniFile.WriteString(NombreUsuario,'ColorFondoGrilla',ColorToString(Grilla.Color));

  IniFile.WriteString(NombreUsuario,'NombreFuenteMemo',NotasUsuario.Font.Name);
  IniFile.WriteInteger(NombreUsuario,'TamanioFuenteMemo', NotasUsuario.Font.Size);
  IniFile.WriteString(NombreUsuario,'ColorFuenteMemo',ColorToString(NotasUsuario.Font.Color));
  IniFile.WriteBool(NombreUsuario,'NegritaFuenteMemo',FsBold in NotasUsuario.Font.Style);
  IniFile.WriteBool(NombreUsuario,'CursivaFuenteMemo',FsItalic in NotasUsuario.Font.Style);
  IniFile.WriteBool(NombreUsuario,'SubrrayadoFuenteMemo',Fsunderline in NotasUsuario.Font.Style);
  IniFile.WriteString(NombreUsuario,'ColorFondoMemo',ColorToString(NotasUsuario.Color));

  IniFile.Free;
End;

Procedure TFormEsba.CambioFuente(Fuente:Tfont);
Begin
   Fuente.Name := MnuFuente.Text;
   Fuente.Size := MnuTamanio.ItemIndex + 8;
   Fuente.Color := MnuColorFuente.Color;
   If MnuNegrita.Down Then
     Fuente.Style := Fuente.Style + [FsBold]
   Else
     Fuente.Style := Fuente.Style - [FsBold];
   If MnuCursiva.Down Then
     Fuente.Style := Fuente.Style + [FsItalic]
   Else
     Fuente.Style := Fuente.Style - [FsItalic];
   If MnuSubrrayado.Down Then
     Fuente.Style := Fuente.Style + [FsUnderline]
   Else
     Fuente.Style := Fuente.Style - [FsUnderline];
   FormEsba.Refresh;
End;

Procedure TFormEsba.AbroBDDAlumnosTHRead(const ACodAlu:String='');
Var
  query, bsq_qr: string;
  SLtxt: TStringList;
  carre: string;
Begin
  SLtxt:= TStringList.Create;
  carre:='';
  if (AnsiContainsText(TxtBusqueda.Text,'_')) then begin
    carre:= AnsiReplaceStr(TxtBusqueda.Text,'_','');
  end
  else begin

    SLtxt.Clear;
    SLtxt.Delimiter := ':';
    SLtxt.StrictDelimiter:= true;
    SLtxt.DelimitedText := TxtBusqueda.Text;
  end;
  try
    query:= 'SELECT A.COD_ALU, A.MATRIZ, A.APELLIDO, A.NOM_APE, A.MAIL, A.FOTO, X.DATOS, X.FCOLOR, X.FMENSAJE, '+
            ' X.OBSERV, A.BAJA, ''(''||A.CARRE||'')''||C.DESCARRE AS CARRERA,C.DESCARRE AS CARRERASC, C.RESOLUCION, A.CARRE AS CODCARRE, A.BAJA, IIF(C.DISTANCIA=''S'',''DISTANCIA'',''PRESENCIAL'') AS MODALIDAD '+
            'FROM ALUMNOS A '+
            ' JOIN CARRERA C ON C.CARRE=A.CARRE '+ ifthen(cbCarreDesuso.Checked,'','AND DESACT=''N'' ')+
            ifthen(FuncionesConfiguracion.Superv,'',' JOIN BARRA_SEGU S ON S.BAROPC=A.CARRE AND S.CODUSU='+IntToStr(FuncionesConfiguracion.CodUsu))+
            ' LEFT OUTER JOIN XXX_OBSERV_PANTA(A.INDICE) X ON A.INDICE=X.XINDICE ';
    bsq_qr:='';
    if ACodAlu<>'' then
      bsq_qr:= ' A.COD_ALU='''+ACodAlu+''''
    else if (carre<>'') then begin
      bsq_qr:= ' A.CARRE='''+carre+''''
    end
    else begin
      if SLtxt[0] <> '' then
        bsq_qr:= ' A.APELLIDO CONTAINING '''+Trim(SLtxt[0])+''' ';
      if (SLtxt.Count>=2) and ((SLtxt[1]) <> '') then
        bsq_qr:= ifthen(bsq_qr='','',bsq_qr+' and')+ ' A.NOM_APE CONTAINING '''+trim(SLtxt[1])+''' ';

       bsq_qr:= ifthen(bsq_qr='','(','(('+bsq_qr+' ) or ')+ ' lower(A.MAIL) CONTAINING '''+LowerCase(trim(SLtxt[0]))+''' OR A.COD_ALU CONTAINING '''+trim(SLtxt[0])+''') ';
    end;

    if chbBuscarBajas.Checked then
      query:= query + ' where ' + bsq_qr +' AND A.BAJA=''S'''
    else
      query:= query + ' where '+ bsq_qr + ' AND A.BAJA=''N''';

    query:= query + ' ORDER BY C.DESCARRE, A.APELLIDO, A.NOM_APE ';
  finally
    SLtxt.Free;
  end;

  FThAlumnos := TFISQLThread.Create(true,1);
  Try
    FrmProgress := TForm.Create(self);
    FrmProgress.Caption := 'Espere';
    FrmProgress.Position := poDesktopCenter;
    FrmProgress.Width := 400;
    FrmProgress.Height := 60;
    FrmProgress.BorderStyle := bsDialog;
    FBarra := TProgressBar.Create(FrmProgress);
    FBarra.Parent := FrmProgress;
    FBarra.Visible := True;
    FBarra.Align := AlClient;
    FBarra.Max := 15000;
    FBarra.Min := 0;
    FBarra.Step := 1;
    FBarra.Smooth := True;

    if ACodAlu<>'' then begin
      FThAlumnos.AKeyUpdate:=ACodAlu;
      FThAlumnos.AFieldKey:='COD_ALU';
    end;

    FThAlumnos.SDataBase:= CustomerData.FBase.DatabaseName;
    FThAlumnos.SUser:= CustomerData.FBase.ConnectParams.UserName;
    FThAlumnos.SPassWord:= CustomerData.FBase.ConnectParams.Password;
    FThAlumnos.SCharSet:= CustomerData.FBase.ConnectParams.CharSet;
    FThAlumnos.ASql:=query;

    Mtalumnos.DisableControls;
    MtAlumnos.Filtered:=False;
    MtAlumnos.OnFilterRecord:=nil;
    MtAlumnos.AfterScroll:=nil;
    MtAlumnos.IndexName:='';
    DataSource1.Enabled:=False;
    DataSource1.DataSet:=nil;
    DbMemo.DataSource:=Nil;
    DbMemo.DataField:='';
    FThAlumnos.AMemTable:=MtAlumnos;
    FThAlumnos.FreeOnTerminate:=True;
    FThAlumnos.onTerminate := ThOnterminated;
    FThALumnos.AProgressBar:=FBarra;
    FThAlumnos.Start;
    FormEsba.Enabled:=False;
    FrmProgress.Show;
  except
    on e:exception do begin
      ShowMessage(e.Message);
      ThOnterminated(self);
    end;
  end;
End;

procedure TFormEsba.ThOnterminated(sender: TObject);
begin
  if FthAlumnos.AErrorMessage<>'' then begin
    ShowMessage(FthAlumnos.AErrorMessage);
    if not MTAlumnos.Active then
      MtALumnos.Active:=True;
  end;

  if Assigned(FrmProgress) then begin
    FrmProgress.Close;
    FreeAndNil(FrmProgress);
  end;
  FormEsba.Enabled:=True;
  FormEsba.Refresh;
  BarradeEstado.Panels.Items[Panel_Registros].Text := 'Total Alumnos: ' + IntToStr(CustomerData.DSAlumnos.RecordCount);
  DataSource1.DataSet:=MtAlumnos;
  DataSource1.Enabled:=True;
  DbMemo.DataSource:=DataSource1;
  DbMemo.DataField:='DATOS';
  Mtalumnos.EnableControls;

  MtAlumnos.Filtered:=True;

  MtAlumnos.AfterScroll:=MtAlumnosAfterScroll;
  MtAlumnos.IndexName:='MtAlumnosIndex1';
  if (FCodAlu<>'') then
    MTAlumnos.Locate('COD_ALU',FCodAlu,[loPartialKey])
  else
    MTAlumnos.First;
  TxtBusqueda.SetFocus;
end;

procedure TFormEsba.NuevoClick(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);

  FrmAltaAlumno.Carrera := SeleccionarCarrera('N');
  if (FrmAltaAlumno.Carrera = '') then
    exit;

  FrmAltaAlumno.Unidad := UnidadPrograma;
  AltaAlumno:= TAltaAlumno.Create(self);

  try
    AltaAlumno.Caption := 'Alta de Alumnos';
    FrmAltaAlumno.Unidad  := UnidadPrograma;
    FrmAltaAlumno.VUsuario := NombreUsuario;
    FrmAltaAlumno.Modificacion := False;
    FrmAltaAlumno.AltaAlumno.CbEstado.ItemIndex:=0;
    FrmAltaAlumno.AltaAlumno.Tipodoc.ItemIndex:=0;
    AltaAlumno.Position:=poScreenCenter;
    AltaAlumno.ShowModal;
    FCodAlu := FrmAltaAlumno.AltaAlumno.Tipodoc.Text+FrmAltaAlumno.AltaAlumno.documento.Text; //CustomerData.DSAlumnos.FieldByName('COD_ALU').AsString;
    if CustomerData.TRAlumnos.Active then
       CustomerData.TRAlumnos.Commit;
    CustomerData.TRAlumnos.Active := False;
    CustomerData.DSAlumnos.Active := False;
    AbroBDDAlumnosTHRead(FCodAlu);
  finally
    AltaAlumno.Free;
    AltaAlumno:=Nil;
  end;
end;


procedure TFormEsba.Timer1Timer(Sender: TObject);
begin
  BarradeEstado.Panels.Items[Panel_Hora].Text := TimeToStr(SysUtils.time);
end;

procedure TFormEsba.dxBarButton1Click(Sender: TObject);
begin
    if MtAlumnos.FieldByName('FMENSAJE').AsString<>'' then
        MessageDlg(Pchar(MtAlumnos.FieldByName('FMENSAJE').AsString),mtInformation,[mbok],0,mbok);
    Ultima_Seleccion(Sender,1);
    InscripcionMaterias := TinscripcionMaterias.Create(Self);
    InscripcionDeMaterias.VUnidad := UnidadPrograma;
    InscripcionDeMaterias.VCarrera := MTAlumnos.FieldByName('CODCARRE').AsString;
    InscripcionDeMaterias.VCodAlu := MTAlumnos.FieldByName('COD_ALU').AsString;
    InscripcionDeMaterias.VApellido := MTAlumnos.FieldByName('APELLIDO').AsString;
    InscripcionDeMaterias.VNombre := MTAlumnos.FieldByName('NOM_APE').AsString;
    InscripcionDeMaterias.VLibroMatriz := MTAlumnos.FieldByName('MATRIZ').AsString;
    InscripcionDeMaterias.VInstituto := 'I. Estudios Superiores Bs. As.';
    InscripcionDeMaterias.VCaracteristica := 'A-781';
    InscripcionDeMaterias.VNombreUsuario := NombreUsuario;
    inscripcionMaterias.Position:=poScreenCenter;
    InscripcionMaterias.ShowModal;

    AbroBDDAlumnosTHRead(FCodAlu);

    InscripcionMaterias.Free;
    InscripcionMaterias:=nil;
end;

procedure TFormEsba.FormClose(Sender: TObject; var Action: TCloseAction);
Begin
 try
    Finalizacion;
 except
    MessageDlg('Hubo error al grabar la configuracion',mtInformation,[mbok],0,mbok);
 end;
 try
    MTAlumnos.Active := False;
 except
    MessageDlg('Hubo error al desactivar la temporal',mtInformation,[mbok],0,mbok);
 end;
 try
   CustomerData.DisConnect;
 except
    MessageDlg('Hubo error al desconectar la base de datos',mtInformation,[mbok],0,mbok);
 end;
// FormEsba.Close;
end;

procedure TFormEsba.SpeedButton4Click(Sender: TObject);
begin
  NotasUsuario.Text := NotasUsuario.Text + TimeToStr(Sysutils.Time);
end;

procedure TFormEsba.SpeedButton1Click(Sender: TObject);
begin
  NotasUsuario.CopyToClipboard;
end;

procedure TFormEsba.SpeedButton2Click(Sender: TObject);
begin
 NotasUsuario.CutToClipboard;
end;

procedure TFormEsba.SpeedButton3Click(Sender: TObject);
begin
 NotasUsuario.PasteFromClipboard;
end;

procedure TFormEsba.FechaMemoCloseUp(Sender: TObject);
begin
  NotasUsuario.Lines.Append(DateToStr(FechaMemo.Date));
end;

procedure TFormEsba.dxBarButton8Click(Sender: TObject);
begin
    if MtAlumnos.FieldByName('FMENSAJE').AsString<>'' then
       MessageDlg(Pchar(MtAlumnos.FieldByName('FMENSAJE').AsString),mtInformation,[mbok],0,mbok);
    Ultima_Seleccion(Sender,1);
    ConstanciadelAlumno2 := TConstanciadelAlumno2.Create(Self); //Creo formulario
    //Paso parametros
    ConstanciaAlumnos2.CarreraActiva := MTAlumnos.FieldByName('CODCARRE').AsString;;
    ConstanciaAlumnos2.NombreCarrera := MTAlumnos.FieldByName('CARRERASC').AsString;;
    ConstanciaAlumnos2.CodigoAlumno := MTAlumnos.FieldByName('COD_ALU').AsString;;
    ConstanciaAlumnos2.UnidadPrograma := UnidadPrograma;
    ConstanciaAlumnos2.ApellidoNombre := MTAlumnos.FieldByName('APELLIDO').AsString+ ' ' + MTAlumnos.FieldByName('NOM_APE').AsString;;
    ConstanciaAlumnos2.LibroMatriz := MTAlumnos.FieldByName('MATRIZ').AsString;;
    ConstanciaDelAlumno2.Position:=poScreenCenter;
    ConstanciaDelAlumno2.ShowModal;
    FCodAlu:=ConstanciaAlumnos2.CodigoAlumno;
    AbroBDDAlumnosTHRead(FCodAlu);
    ConstanciaDelAlumno2.Free;
    ConstanciaDelAlumno2:=nil;
end;

procedure TFormEsba.dxBarButton9Click(Sender: TObject);
begin
     Ultima_Seleccion(Sender,1);
     ConstAlumnoRegular := TConstAlumnoRegular.Create(Self);
     constanciaalumnoregular.UnidadPrograma := UnidadPrograma;
     constanciaalumnoregular.CarreraActiva := MTAlumnos.FieldByName('CODCARRE').AsString;;
     constanciaalumnoregular.CodigoAlumno := MTAlumnos.FieldByName('COD_ALU').AsString;
     constanciaalumnoregular.Apellidonombre := MTAlumnos.FieldByName('APELLIDO').AsString+ ' ' + MTAlumnos.FieldByName('NOM_APE').AsString;
     constanciaalumnoregular.Carreralarga := MTAlumnos.FieldByName('CARRERASC').AsString;;
     constanciaalumnoregular.Usuario := NombreUsuario;
     constanciaalumnoregular.MailAlumno:= MtAlumnos.FieldByName('MAIL').AsString;
     ConstAlumnoRegular.Position:=poScreenCenter;
     ConstAlumnoRegular.ShowModal;
     ConstAlumnoRegular.Free;
     FCodAlu:=constanciaalumnoregular.CodigoAlumno;
     AbroBDDAlumnosTHRead(FCodAlu);
end;

procedure TFormEsba.GrillaMouseUp(Sender: TObject; Button: TMouseButton;
  Shift: TShiftState; X, Y: Integer);
begin
 If Button = mbRight Then
  Begin
    MenuContextual := '';
    Mnufuente.Text :=  Grilla.Font.Name;
    MnuColorFuente.Color := Grilla.Font.Color;
    MnuTamanio.ItemIndex := Grilla.Font.Size - 8;
    MnuColorFondo.Color := Grilla.Color;
    MnuNegrita.Down   := FsBold In Grilla.Font.Style;
    MnuCursiva.Down   := FsItalic In Grilla.Font.Style;
    MnuSubrrayado.Down := FsUnderline In Grilla.Font.Style;
    MenuContextual := 'GRILLA';
    Contextual.PopupFromCursorPos;
  End;
end;

procedure TFormEsba.GrillaTitleClick(Column: TColumn);
begin
  if MtAlumnos.IndexName = Column.FieldName then
    MtAlumnos.IndexName:= Column.FieldName+'_DESC'
  else
    MtAlumnos.IndexName:= Column.FieldName;
end;

procedure TFormEsba.ModificarAlumnoClick(Sender: TObject);
Var
  Query:WideString;
begin
     Ultima_Seleccion(Sender,1);
     FrmAltaAlumno.VUsuario := NombreUsuario;
     FrmAltaAlumno.Carrera := MTAlumnos.FieldByName('CODCARRE').AsString;
     AltaAlumno:= TAltaAlumno.Create(self);
     AltaAlumno.Caption := 'Modificacion de Alumno';
     FrmAltaAlumno.Modificacion := True;
     //CARGO DATOS DEL ALUMNOS EN EL FORMULARIO
     if CustomerData.TrDsVarios.Active then
           CustomerData.TrDsVarios.Rollback;
     CustomerData.IbDsVarios.Active:=False;
     Query:= 'SELECT A.COD_ALU, MATRIZ, APELLIDO, NOM_APE,MAIL, EXT_POR, NACIONAL, '+
             'EST_CIV, SEXO, FEC_NAC, LUG_NAC,PCIA_NAC, DOMI, LOCALI, COD_POS, CAR_TEL, TELE, '+
             'FEC_ING, CTT, CA, DNI, FECH_CTT,EST_CIV,CPRIM,APRIM,TPRIM,CSECU, '+
             'ASECU,TSECU,TERCI,ATERCI,TTERCI,EMPRE,RUBRO,CARGO,ANTI,DOMI_1,TELE_1,INTER, '+
             'BAJA, CELULAR, FFOTO, FAPTFIS, FAPTFEC, FOTO, FUSUWEB, FPARNAC, FLIBRETA, FLIBFEC, ESTADO, GENERO, NOMIPASE ' +
             'FROM ALUMNOS A  '+
             'WHERE CARRE = '+#39+MTAlumnos.FieldByName('CODCARRE').AsString+#39+' AND BAJA = '+#39+MTAlumnos.FieldByName('BAJA').AsString+#39+
             ' AND COD_ALU='#39+MTAlumnos.FieldByName('COD_ALU').AsString+#39;
     CustomerData.IbDsVarios.SQLs.SelectSQL.Text:= Query;
     CustomerData.TrDsVarios.Active:=True;
     CustomerData.IbDsVarios.Active:=True;
     With AltaAlumno Do
      Begin
       TipoDoc.ItemIndex          := TipoDoc.Items.IndexOf(Copy(CustomerData.IbDsVarios.FieldByName('COD_ALU').AsString,1,3));
       Documento.Text             := Copy(CustomerData.IbDsVarios.FieldByName('COD_ALU').AsString,4,11);
       Apellido.Text              := CustomerData.IbDsVarios.FieldByName('APELLIDO').AsString;
       Nombre.Text                := CustomerData.IbDsVarios.FieldByName('NOM_APE').AsString;
       Nacionalidad.Text          := CustomerData.IbDsVarios.FieldByName('NACIONAL').AsString;
       If CustomerData.IbDsVarios.FieldByName('Est_Civ').AsString = 'S' Then
           EstadoCivil.ItemIndex  := 0
       Else
         If CustomerData.IbDsVarios.FieldByName('Est_Civ').AsString = 'C' Then
            EstadoCivil.ItemIndex  := 1
         Else
           If CustomerData.IbDsVarios.FieldByName('Est_Civ').AsString = 'V' Then
             EstadoCivil.ItemIndex  := 3
           Else
              If CustomerData.IbDsVarios.FieldByName('Est_Civ').AsString = 'D' Then
                EstadoCivil.ItemIndex  := 2
              Else
                EstadoCivil.ItemIndex  := -1;
       If (CustomerData.IbDsVarios.FieldByname('Sexo').AsString='M') Then
          CmbSexo.ItemIndex:=1
       else
          If (CustomerData.IbDsVarios.FieldByname('Sexo').AsString='F') Then
             CmbSexo.ItemIndex:=0;
       FechaNacimiento.Date       := CustomerData.IbDsVarios.FieldByName('Fec_Nac').AsDateTime;
       LugarNacimiento.Text       := CustomerData.IbDsVarios.FieldByName('Lug_Nac').AsString;
       ProvinciaNacimiento.Text   := CustomerData.IbDsVarios.FieldByName('Pcia_Nac').AsString;
       Domicilio.Text             := CustomerData.IbDsVarios.FieldByName('DOMI').AsString;
       Localidad.Text             := CustomerData.IbDsVarios.FieldByName('LOCALI').AsString;
       CodigoPostal.Text          := IntToStr(CustomerData.IbDsVarios.FieldByName('COD_POS').AsInteger);
       Caracteristica.Text        := CustomerData.IbDsVarios.FieldByName('Car_Tel').AsString;
       Telefono.Text              := CustomerData.IbDsVarios.FieldByName('Tele').AsString;
       FechaIngreso.Date          := CustomerData.IbDsVarios.FieldByName('Fec_ing').AsDateTime;
       ColegioPrimario.Text       := CustomerData.IbDsVarios.FieldByName('CPrim').AsString;
       AnosPrimario.Text          := CustomerData.IbDsVarios.FieldByName('APrim').AsString;
       ModalidadPrimario.Text     := CustomerData.IbDsVarios.FieldByName('TPrim').AsString;
       ColegioSecundario.Text     := CustomerData.IbDsVarios.FieldByName('Csecu').AsString;
       AnosSecundario.Text        := CustomerData.IbDsVarios.FieldByName('ASecu').AsString;
       ModalidadSecundario.Text   := CustomerData.IbDsVarios.FieldByName('TSecu').AsString;
       ColegioTerciario.Text      := CustomerData.IbDsVarios.FieldByName('Terci').AsString;
       AnosTerciario.Text         := CustomerData.IbDsVarios.FieldByName('ATerci').AsString;
       ModalidadTerciario.Text    := CustomerData.IbDsVarios.FieldByName('TTerci').AsString;
       Empresa.Text               := CustomerData.IbDsVarios.FieldByName('Empre').AsString;
       Rubro.Text                 := CustomerData.IbDsVarios.FieldByName('Rubro').AsString;
       Cargo.Text                 := CustomerData.IbDsVarios.FieldByName('Cargo').AsString;
       Antiguedad.Text            := CustomerData.IbDsVarios.FieldByName('Anti').AsString;
       Direccion.Text             := CustomerData.IbDsVarios.FieldByName('Domi_1').AsString;
       TelefonoLaboral.Text       := CustomerData.IbDsVarios.FieldByName('Tele_1').AsString;
       Interno.Text               := CustomerData.IbDsVarios.FieldByName('Inter').AsString;
       Mail.Text                  := CustomerData.IbDsVarios.FieldByName('Mail').AsString;
       //Memo.Lines.Text           := CustomerData.IbDsVarios.FieldByName('OBSERV').AsString;
       celular.text := CustomerData.IbDsVarios.FieldByName('CELULAR').AsString;
       if CustomerData.IbDsVarios.FieldByName('ESTADO').Value = 'C' then //cursando
         CbEstado.ItemIndex:=0
       else if CustomerData.IbDsVarios.FieldByName('ESTADO').Value = 'P' then
         CbEstado.ItemIndex:=1
       else if CustomerData.IbDsVarios.FieldByName('ESTADO').Value = 'E' then
         CbEstado.ItemIndex:=2
       else if CustomerData.IbDsVarios.FieldByName('ESTADO').Value = 'N' then
         CbEstado.ItemIndex:=3;

       If CustomerData.IbDsVarios.FieldByName('BAJA').AsString='S' Then
           CbxBaja.Checked            := True
       else
           CbxBaja.Checked            := False;
       If CustomerData.IbDsVarios.FieldByName('CTT').AsString = '*' Then
        Begin
          Ctt.Checked             := True;
          Fechactt.Date           := CustomerData.IbDsVarios.FieldByName('Fech_Ctt').AsDateTime;
        End;
       If CustomerData.IbDsVarios.FieldByName('FAPTFIS').AsString = 'S' Then
        Begin
          Aptofisico.Checked             := True;
          FechaAptofisico.Date           := CustomerData.IbDsVarios.FieldByName('FAPTFEC').AsDateTime;
        End;
        If CustomerData.IbDsVarios.FieldByName('FFOTO').AsString = 'S' Then
          Foto.Checked             := True
        else
          Foto.Checked             := False;
       If CustomerData.IbDsVarios.FieldByName('FPARNAC').AsString = 'S' Then
          Pn.Checked             := True
        else
          Pn.Checked             := False;

       If CustomerData.IbDsVarios.FieldByName('NOMIPASE').AsString = 'S' Then
          nominaPase.Checked             := True
        else
          nominaPase.Checked             := False;

       If CustomerData.IbDsVarios.FieldByName('DNI').AsString <> '' Then
          DNI.ItemIndex             := StrToInt(CustomerData.IbDsVarios.FieldByName('DNI').AsString);
       If CustomerData.IbDsVarios.FieldByName('CA').AsString = '*' Then
          CA.Checked              := True;
       if not customerdata.ibdsvarios.fieldbyname('FOTO').IsNull then
          image.Picture:=imagen.Picture
       else
          image.Picture:=nil;
       if not CustomerData.Ibdsvarios.FieldByName('GENERO').IsNull then
          CbGenero.ItemIndex:=CustomerData.Ibdsvarios.FieldByName('GENERO').AsInteger
       else
          CbGenero.ItemIndex:=-1;
       CbLIbreta.Checked := (customerdata.ibdsvarios.fieldbyname('FLIBRETA').AsString='S');
       FechaLibreta.Date := CustomerData.IbDsVarios.FieldByName('FLIBFEC').AsDateTime;
       CbHabilitarWeb.Checked := false;
       if (customerdata.ibdsvarios.fieldbyname('FUSUWEB').AsString = 'S') then
        CbHabilitarWeb.Checked := true;

     End;
     //FIN CARGA DE DATOS DEL FORMULARIO
     AltaAlumno.Position:=poScreenCenter;
     AltaAlumno.ShowModal;
     FCodAlu := FrmAltaAlumno.AltaAlumno.Tipodoc.Text+FrmAltaAlumno.AltaAlumno.documento.Text; //CustomerData.DSAlumnos.FieldByName('COD_ALU').AsString;
     AbroBDDAlumnosTHRead(FCodAlu);
     AltaAlumno.Free;
end;

procedure TFormEsba.MtAlumnosAfterScroll(DataSet: TDataSet);
var
   m : TStream;
   FirstBytes: AnsiString;
begin
  LblApellidoNombre.Caption:= MtAlumnosAPELLIDO.AsString+', '+MtAlumnosNOM_APE.AsString;
  LblCarrera.Caption:= MtAlumnosCARRERA.AsString+' '+MtAlumnosRESOLUCION.AsString;
  if Mtalumnos.FieldByName('FOTO').IsNull then begin
    imagen.Picture:=image1.picture;
    exit;
  end;
  m := Mtalumnos.CreateBlobStream(MtAlumnosFOTO,bmRead);
  if m <> nil then begin
    try
      try
        SetLength(FirstBytes, 8);
        m.Read(FirstBytes[1], 8);
        if Copy(FirstBytes, 1, 2) = 'BM' then
          Imagen.Picture.Graphic := TBitmap.Create
        else if FirstBytes = #137'PNG'#13#10#26#10 then
          Imagen.Picture.Graphic := TPngImage.Create
        else if Copy(FirstBytes, 1, 3) = 'GIF' then
          Imagen.Picture.Graphic := TGIFImage.Create
        else if Copy(FirstBytes, 1, 2) = #$FF#$D8 then
          Imagen.Picture.Graphic := TJPEGImage.Create;
        m.Seek(0,0);
        Imagen.Picture.Graphic.LoadFromStream(m);
      finally
        m.Free;
      end;
    except
      on e:exception do begin
        imagen.Picture:=image1.picture;
        ShowMessage('Error al cargar la imagen. '+#13+e.Message);
      end;
    end;
  end;
end;

procedure TFormEsba.GrillaDblClick(Sender: TObject);
begin
 ModificarAlumnoClick(Sender);
end;

procedure TFormEsba.GrillaKeyPress(Sender: TObject; var Key: Char);
begin
 Case Key Of
   #13: ModificarAlumnoClick(Sender);
 end;
end;

procedure TFormEsba.SalirClick(Sender: TObject);
begin
  Finalizacion;
  Application.Terminate;
end;

procedure TFormEsba.dxBarButton12Click(Sender: TObject);
var
  carDes: string;
begin
 Ultima_Seleccion(Sender,1);
 AltaModifMaterias.CarreraActiva := SeleccionarCarrera('',carDes);
 if (AltaModifMaterias.CarreraActiva = '') then
  exit;
 AlMoMaterias := TAlMoMaterias.Create(Self);
 try
   AltaModifMaterias.UsuarioActivo := NombreUsuario;
   AltaModifMaterias.CarreraLarga := '';
   AlMoMaterias.Position:=poScreenCenter;
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781 '+ carDes ;
   AlMoMaterias.ShowModal;
 finally
  FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781';
   AlmoMaterias.Free;
 end;

end;

procedure TFormEsba.Mesasdeexamen1Click(Sender: TObject);
var
  carDes: string;
begin
 Ultima_Seleccion(Sender,1);
 MesasExamen.VCarrera := SeleccionarCarrera('', carDes);
 if (MesasExamen.VCarrera = '') then
  exit;
 mesasexamenes := Tmesasexamenes.Create(Self);
 try
   MesasExamen.VUnidad := UnidadPrograma;
   MesasExamen.VNombreUsuario := NombreUsuario;
   mesasexamenes.Position:=poScreenCenter;
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781 '+ carDes;
   mesasexamenes.ShowModal;
 finally
  FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781';
  mesasexamenes.Free;
 end;
end;

procedure TFormEsba.MateriasporComision1Click(Sender: TObject);
var
  carDes: string;
begin
 Ultima_Seleccion(Sender,1);
 RegularizacionDeMateriasXComision_nuevo.VCarrera := SeleccionarCarrera('N',carDes);
 if (RegularizacionDeMateriasXComision_nuevo.VCarrera = '') then
   exit;
 regularizacionMateriasXComision_nuevo := TregularizacionMateriasXComision_nuevo.Create(Self);
 try
   RegularizacionDeMateriasXComision_nuevo.VUnidad := UnidadPrograma;
   RegularizacionDeMateriasXComision_nuevo.VInstituto := 'I. Estudios Superiores Bs. As.';
   RegularizacionDeMateriasXComision_nuevo.VCaracteristica := 'A-781';
   RegularizacionDeMateriasXComision_nuevo.VCuatrimestreAnio := '';
   RegularizacionDeMateriasXComision_nuevo.VNombreUsuario := NombreUsuario;
   regularizacionmateriasxcomision_nuevo.Position:=poScreenCenter;
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781 '+ carDes;
   regularizacionMateriasXComision_nuevo.ShowModal;
 finally
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781';
   regularizacionMateriasXComision_nuevo.Free;
   regularizacionMateriasXComision_nuevo:=nil;
 end;
end;

procedure TFormEsba.dxBarButton2Click(Sender: TObject);
begin
    if MtAlumnos.FieldByName('FMENSAJE').AsString<>'' then
       MessageDlg(Pchar(MtAlumnos.FieldByName('FMENSAJE').AsString),mtInformation,[mbok],0,mbok);
    Ultima_Seleccion(Sender,1);
    RegularizacionDeMaterias_nuevo.VCarrera := MTAlumnos.FieldByName('CODCARRE').AsString; //SeleccionarCarrera('');
    if (RegularizacionDeMaterias_nuevo.VCarrera = '') then
      exit;

    regularizacionMaterias_nuevo := TregularizacionMaterias_nuevo.Create(Self);
    try
      RegularizacionDeMaterias_nuevo.VUnidad := UnidadPrograma;

      RegularizacionDeMaterias_nuevo.VCodAlu := MTAlumnos.FieldByName('COD_ALU').AsString;
      RegularizacionDeMaterias_nuevo.VApellido := MTAlumnos.FieldByName('APELLIDO').AsString;
      RegularizacionDeMaterias_nuevo.VNombre := MTAlumnos.FieldByName('NOM_APE').AsString;
      RegularizacionDeMaterias_nuevo.VLibroMatriz := MTAlumnos.FieldByName('MATRIZ').AsString;
      RegularizacionDeMaterias_nuevo.VInstituto := 'I. Estudios Superiores Bs. As.';
      RegularizacionDeMaterias_nuevo.VCaracteristica := 'A-781';
      RegularizacionDeMaterias_nuevo.VNombreUsuario := NombreUsuario;
      RegularizacionMaterias_nuevo.Position:=poScreenCenter;
      RegularizacionMaterias_nuevo.ShowModal;
      FcodAlu:=RegularizacionDeMaterias_nuevo.VCodAlu;
      AbroBDDAlumnosTHRead(FCodAlu);
    finally
      RegularizacionMaterias_nuevo.Free;
      RegularizacionMaterias_nuevo:=nil;
    end;
end;

procedure TFormEsba.dxBarButton3Click(Sender: TObject);
begin
    if MtAlumnos.FieldByName('FMENSAJE').AsString<>'' then
       MessageDlg(Pchar(MtAlumnos.FieldByName('FMENSAJE').AsString),mtInformation,[mbok],0,mbok);
    Ultima_Seleccion(Sender,1);
    PermisosExamen := TPermisosExamen.Create(Self);
    try
      PermisoExamen.VUnidad := UnidadPrograma;
      PermisoExamen.VCarrera := MTAlumnos.FieldByName('CODCARRE').AsString;
      PermisoExamen.VCodAlu := MTAlumnos.FieldByName('COD_ALU').AsString;
      PermisoExamen.VApellido := MTAlumnos.FieldByName('APELLIDO').AsString;
      PermisoExamen.VNombre := MTAlumnos.FieldByName('NOM_APE').AsString;
      PermisoExamen.VLibroMatriz := MTAlumnos.FieldByName('MATRIZ').AsString;
      PermisoExamen.VInstituto := 'I. Estudios Superiores Bs. As.';
      PermisoExamen.VCaracteristica := 'A-781';
      PermisoExamen.VNombreUsuario := NombreUsuario;
      PermisosExamen.Position:=poScreenCenter;
      PermisosExamen.ShowModal;
      FCodAlu:=PermisoExamen.VCodAlu;
      AbroBDDAlumnosTHRead(FCodAlu);
    finally
      PermisosExamen.Free;
    end;
end;

procedure TFormEsba.MnuAltaUsuarioClick(Sender: TObject);
begin
  FrmAltaUsuario := TFrmAltaUsuario.Create(Self);
  try
    AltaUsuario.VUnidad := UnidadPrograma;
    FrmAltaUsuario.Position:=poScreenCenter;
    FrmAltaUsuario.ShowModal;
  finally
    FrmAltaUsuario.Free;
  end;
end;

procedure TFormEsba.mnuBajaUsuariosClick(Sender: TObject);
begin
  FrmBajaUsuarios := TFrmBajaUsuarios.Create(Self);
  try
    BajaUsuarios.Vunidad := UnidadPrograma;
    FrmBajaUsuarios.Position:=poScreenCenter;
    FrmBajaUsuarios.ShowModal;
  finally
    FrmBajaUsuarios.Free;
  end;
end;

procedure TFormEsba.mnucambiopassClick(Sender: TObject);
begin
FrmCambioPass := TFrmCambioPass.Create(Self);
try
  CambioPassword.VCodUsu := FuncionesConfiguracion.CodUsu;
  FrmCambioPass.Position:=poScreenCenter;
  FrmCambioPass.ShowModal;
finally
  FrmCambioPass.Free;
end;
end;

procedure TFormEsba.mnucercervClick(Sender: TObject);
begin
  FrmCertServ := TFrmCertServ.Create(Self);
  try
    CertServ.VUsuario := NombreUsuario;
    FrmCertServ.Position:=poScreenCenter;
    FrmCertServ.ShowModal;
  finally
    FrmCertServ.Free;
  end;

end;

procedure TFormEsba.mnudocentesClick(Sender: TObject);
begin
  FrmAltaModifProf := TFrmAltaModifProf.Create(Self);
  try
    FrmAltaModProfes.VUsuario := NombreUsuario;
    FrmAltaModifProf.ShowModal;
  finally
    FrmAltaModifProf.Free;
  end;

end;

procedure TFormEsba.mnuListadoDocentesClick(Sender: TObject);
begin
  ListadoDinamico(Sender);
end;

procedure TFormEsba.dxBarButton41Click(Sender: TObject);
var
  carDes: string;
  FrmPermisos: TFrmCargaPermisosWeb;
begin
 Ultima_Seleccion(Sender,1);
 CargadePermisosWeb.VCarrera := SeleccionarCarrera('', carDes);
 if (CargadePermisosWeb.VCarrera='') then
  exit;
 FrmPermisos := TFrmCargaPermisosWeb.Create(Self);
 try
   CargadePermisosWeb.VUnidad := UnidadPrograma;
   CargadePermisosWeb.VInstituto := 'I. Estudios Superiores Bs. As.';
   CargadePermisosWeb.VCaracteristica := 'A-781';
   CargadePermisosWeb.VNombreUsuario := NombreUsuario;

   FrmPermisos.Position:=poScreenCenter;
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781 '+ carDes;
   FrmPermisos.CargaGrilla;
   FrmPermisos.ShowModal;
 finally
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781';
   FrmPermisos.Free;
 end;

end;

procedure TFormEsba.dxBarButton42Click(Sender: TObject);
var
  carDes: string;
  FrmInscripciones: TFrmInscripcionesWeb;
begin
 Ultima_Seleccion(Sender,1);
 InscripcionesWeb.VCarrera := SeleccionarCarrera('', carDes);
 if (InscripcionesWeb.VCarrera='') then
  exit;
 FrmInscripciones := TFrmInscripcionesWeb.Create(Self);
 try
   InscripcionesWeb.VUnidad := UnidadPrograma;
   InscripcionesWeb.VInstituto := 'I. Estudios Superiores Bs. As.';
   InscripcionesWeb.VCaracteristica := 'A-781';
   InscripcionesWeb.VNombreUsuario := NombreUsuario;

   FrmInscripciones.Position:=poScreenCenter;
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781 '+ carDes;
   FrmInscripciones.CargaGrilla;
   FrmInscripciones.ShowModal;
 finally
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781';
   FrmInscripciones.Free;
 end;


end;

procedure TFormEsba.dxBarButton43Click(Sender: TObject);
begin

     CustomerData.IBUpVarios.Sql.Text:='SELECT FERRCOD, FERRMSG FROM XXX_BORRA_ALUMNO('+#39+MTAlumnos.FieldByName('CODCARRE').AsString+#39+','+#39+MTAlumnos.FieldByName('COD_ALU').AsString+#39+','+IntToStr(FuncionesConfiguracion.CodUsu)+')';
     CustomerData.TrUpVarios.Active:=True;
     CustomerData.IBUpVarios.ExecQuery;
     If (CustomerData.IBupVarios.FieldByName('FERRCOD').AsInteger=2) Then Begin
            MessageDlg(PChar(NombreUsuario + ', '+CustomerData.IBUpVarios.FieldByName('FERRMSG').AsString),mtError,[mbok],0,mbok);
            CustomerData.TRUpVarios.Rollback;
            CustomerData.TRUpVarios.Active := False;
            CustomerData.IBUpVarios.Close;
     End
     ELSE If (CustomerData.IBUpVarios.FieldByName('FERRCOD').AsInteger=1) and
             (MessageDlg(PChar(NombreUsuario + ', '+CustomerData.IBUpVarios.FieldByName('FERRMSG').AsString),mtConfirmation,[mbyes,mbno],0,mbyes)=mryes) then begin
              CustomerData.TRUpVarios.Commit;
              CustomerData.TRUpVarios.Active := False;
              CustomerData.IBUpVarios.Close;
     End
     else If (CustomerData.IBUpVarios.FieldByName('FERRCOD').AsInteger=0) then begin
              CustomerData.TRUpVarios.Commit;
              CustomerData.TRUpVarios.Active := False;
              CustomerData.IBUpVarios.Close;
     end
     else begin
            CustomerData.TRUpVarios.Rollback;
            CustomerData.TRUpVarios.Active := False;
            CustomerData.IBUpVarios.Close;
     end;
 AbroBDDAlumnosTHRead();
end;

procedure TFormEsba.dxBarButton45Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:=MTAlumnos.FieldByName('CODCARRE').AsString;
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Año Carrera";ANIO;K;S;"SELECT DISTINCT CUATRIM FROM MATERIAS M WHERE M.CODCARRE='+#39+MTAlumnos.FieldByName('CODCARRE').AsString+#39+'";"Año";;CUATRIM;'+#13+
                                     'Imp.Sanciones;IMPS;L;S;Si,No;S,N;0';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    Impresiones.Imp_Boletin_calif_formatoNuevo(MTAlumnos.FieldByName('CODCARRE').AsString,MTAlumnos.FieldByName('COD_ALU').AsString,UnidadPrograma,MTAlumnos.FieldByName('BAJA').AsString,StrToInt(TComboEdit(FrmParametros.LstParam[0].control).Text),0,TComboBox(FrmParametros.LstParam[1].Control).ItemIndex);
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton4Click(Sender: TObject);
begin

    Ultima_Seleccion(Sender,1);
    NotasExamenFinal.VUnidad := UnidadPrograma;
    NotasExamenFinal.VCarrera := MTAlumnos.FieldByName('CODCARRE').AsString;
    NotasExamenFinal.VCodAlu := MTAlumnos.FieldByName('COD_ALU').AsString;
    NotasExamenFinal.VApellido := MTAlumnos.FieldByName('APELLIDO').AsString;
    NotasExamenFinal.VNombre := MTAlumnos.FieldByName('NOM_APE').AsString;
    NotasExamenFinal.VLibroMatriz := MTAlumnos.FieldByName('MATRIZ').AsString;
    NotasExamenFinal.VInstituto := 'I. Estudios Superiores Bs. As.';
    NotasExamenFinal.VCaracteristica := 'A-781';
    NotasExamenFinal.VNombreUsuario := NombreUsuario;
    NotasExamenes := TNotasExamenes.Create(Self);
    try
      notasexamenes.Position:=poScreenCenter;
      NotasExamenes.ShowModal;
      FCodAlu:=NotasExamenFinal.VCodAlu;
      AbroBDDAlumnosTHRead(FCodAlu);
    finally
      NotasExamenes.Free;
    end;
end;

procedure TFormEsba.dxBarButton5Click(Sender: TObject);
begin
    Ultima_Seleccion(Sender,1);
    FRMEquivalencias := TFRMEquivalencias.Create(Self);
    try
      Equivalencia.VUnidad := UnidadPrograma;
      Equivalencia.VCarrera := MTAlumnos.FieldByName('CODCARRE').AsString;
      Equivalencia.VCodAlu := MTAlumnos.FieldByName('COD_ALU').AsString;
      Equivalencia.VApellido := MTAlumnos.FieldByName('APELLIDO').AsString;
      Equivalencia.VNombre := MTAlumnos.FieldByName('NOM_APE').AsString;
      Equivalencia.VLibroMatriz := MTAlumnos.FieldByName('MATRIZ').AsString;
      Equivalencia.VInstituto := 'I. Estudios Superiores Bs. As.';
      Equivalencia.VCaracteristica := 'A-781';
      Equivalencia.VNombreUsuario := NombreUsuario;
      FRMEquivalencias.Position:=poScreenCenter;
      FRMEquivalencias.ShowModal;
      FCodAlu:=Equivalencia.VCodAlu;
      AbroBDDAlumnosTHRead(FCodAlu);
    finally
      FRMEquivalencias.Free;
    end;
end;

procedure TFormEsba.dxBarButton13Click(Sender: TObject);
begin
    Ultima_Seleccion(Sender,1);
    ModifCodAlu_LM := TModifCodAlu_LM.Create(Self);
    try
      CambioCodAlu_LM.UnidadPrograma  := UnidadPrograma;
      CambioCodAlu_LM.CarreraActiva := MtAlumnos.FieldByName('CODCARRE').AsString;
      CambioCodAlu_LM.CodigoAlumnoViejo := MtAlumnos.FieldByName('COD_ALU').AsString;
      CambioCodAlu_LM.LibroMatrizViejo := MtAlumnos.FieldByName('MATRIZ').AsString;
      CambioCodAlu_LM.UsuarioActivo  := NombreUsuario;
      CambioCodAlu_LM.TipodeCambio := 'MATRIZ';
      ModifCodAlu_LM.Position:=poScreenCenter;

      ModifCodAlu_LM.Caption := 'Modificacion del libro matriz';
      ModifCodAlu_LM.Label3.Visible  := False;
      ModifCodAlu_LM.Tipodoc.Visible := False;
      ModifCodAlu_LM.Label4.Caption  := 'Libro Matriz';
      ModifCodAlu_LM.Label4.Left     := 6;
      ModifCodAlu_LM.CodAlu_NumLibro.Left := 72;
      ModifCodAlu_LM.Cambiar.Left := 168;
      ModifCodAlu_LM.Salir.Left := 248;
      ModifCodAlu_LM.Width := 341;
      ModifCodAlu_LM.CodAlu_NumLibro.MaxLength := 2;
      ModifCodAlu_LM.NumeroFolio.MaxLength := 3;
      ModifCodAlu_LM.CodAlu_NumLibro.Width := 57;

      ModifCodAlu_LM.Label1.Top := 23;
      ModifCodAlu_LM.NumeroFolio.Top := 15;
      ModifCodAlu_LM.Label1.Visible := True;
      ModifCodAlu_LM.NumeroFolio.Visible := True;

      ModifCodAlu_LM.ShowModal;
      AbroBDDAlumnosTHRead(FCodAlu);
    finally
      ModifCodAlu_LM.Free;
      ModifCodAlu_LM:=nil;
    end;
end;

procedure TFormEsba.dxBarButton14Click(Sender: TObject);
begin
    Ultima_Seleccion(Sender,1);
    ModifCodAlu_LM := TModifCodAlu_LM.Create(Self);
    try
      CambioCodAlu_LM.UnidadPrograma  := UnidadPrograma;
      CambioCodAlu_LM.CarreraActiva := MtAlumnos.FieldByName('CODCARRE').AsString;
      CambioCodAlu_LM.CodigoAlumnoViejo :=  MtAlumnos.FieldByName('COD_ALU').AsString;
      CambioCodAlu_LM.LibroMatrizViejo := MtAlumnos.FieldByName('MATRIZ').AsString;
      CambioCodAlu_LM.UsuarioActivo  := NombreUsuario;
      CambioCodAlu_LM.TipodeCambio := 'Cod_Alu';
      ModifCodAlu_LM.Caption := 'Modificacion del codigo de alumno';
      ModifCodalu_LM.Position:=poScreenCenter;
      ModifCodAlu_LM.ShowModal;
      AbroBDDAlumnosTHRead(FCodAlu);
    finally
      ModifCodAlu_LM.Free;
      ModifCodAlu_LM:=nil;
    end;
end;

procedure TFormEsba.dxBarButton10Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  AMAnalitico := TAMAnalitico.Create(Self);
  try
    ModifAnalitico.VUnidad := UnidadPrograma;
    ModifAnalitico.VCarrera := MtAlumnos.FieldByName('CODCARRE').AsString;
    ModifAnalitico.VCodAlu := MtAlumnos.FieldByName('COD_ALU').AsString;
    ModifAnalitico.VApellido := MtAlumnos.FieldByName('APELLIDO').AsString;
    ModifAnalitico.VNombre := MtAlumnos.FieldByName('NOM_APE').AsString;
    ModifAnalitico.VLibroMatriz :=MtAlumnos.FieldByName('MATRIZ').AsString;
    ModifAnalitico.VInstituto := 'I. Estudios Superiores Bs. As.';
    ModifAnalitico.VCaracteristica := 'A-781';
    ModifAnalitico.VNombreUsuario := NombreUsuario;
    AMAnalitico.Position:=poScreenCenter;
    AMAnalitico.ShowModal;
    FCodALu:=ModifAnalitico.VCodAlu;
    AbroBDDAlumnosTHRead(FCodAlu);
  finally
    AMAnalitico.Free;
    AMAnalitico:=Nil;
  end;

end;

procedure TFormEsba.dxBarSubItem5Popup(Sender: TObject);
begin
  dxBarButton11.Enabled := MtAlumnos.FieldByName('MAIL').AsString <> '';
  dxBarButton11.Caption := '&A ' + MtAlumnos.FieldByName('APELLIDO').AsString  + ', ' + MtAlumnos.FieldByName('NOM_APE').AsString;
end;

procedure TFormEsba.dxBarTablaDeConfiguracionesClick(Sender: TObject);
var
   _form: TFrmTablaConfiguracion;
begin
   Ultima_Seleccion(Sender,1);
  try
    CargadeTrimestres.modo:='CUATRIMESTRAL';
    _form:=TFrmTablaConfiguracion.Create(self);

    _form.Position:=poScreenCenter;
    _form.ShowModal;
  finally
    _form.Free;
    _form:=Nil;
  end;
end;

procedure TFormEsba.dxBarButton11Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  EnvioCorreo.Vcarrera := MTAlumnos.FieldByName('CODCARRE').AsString;

  Frmenviomail := TFrmenviomail.Create(Self);
  try
    EnvioCorreo.VUnidad := UnidadPrograma;

    EnvioCorreo.VUsuario := NombreUsuario;
    FrmEnvioMail.DEPara.Text := MtAlumnos.FieldByName('MAIL').AsString;
    FrmEnvioMail.Position:=poScreenCenter;
    FrmEnvioMail.ShowModal;
  finally
    FrmEnvioMail.Free;
  end;

end;

procedure TFormEsba.ContextualPopup(Sender: TObject);
begin
 dxBarButton11.Enabled := MtAlumnos.FieldByName('MAIL').AsString <> '';
 dxBarButton11.Caption := '&Enviar correo a ' + MtAlumnos.FieldByName('APELLIDO').AsString  + ', ' + MtAlumnos.FieldByName('NOM_APE').AsString;
end;

procedure TFormEsba.DocentesporComision1Click(Sender: TObject);
begin
 Ultima_Seleccion(Sender,1);
 CargaComisiones.VCarrera := SeleccionarCarrera('N');
 if (CargaComisiones.VCarrera='') then
  exit;

 FrmComArmadas := TFrmComArmadas.Create(Self);
 try
   CargaComisiones.VUnidad := UnidadPrograma;
   CargaComisiones.VInstituto := 'I. Estudios Superiores Bs. As.';
   CargaComisiones.VCaracteristica := 'A-781';
   CargaComisiones.VCuatrimestreAnio := FuncionesText.Cuatrimestre(vcarrera);
   CargaComisiones.VNombreUsuario := NombreUsuario;
   if (CargaComisiones.VCarrera = '333') or (CargaComisiones.VCarrera = '650') Then
      CargaComisiones.Filas := 3
   else
      CargaComisiones.Filas := 2;
   FrmComArmadas.Position:=poScreenCenter;
   FrmComArmadas.ShowModal;
 finally
   FrmComArmadas.Free;
 end;
end;

procedure TFormEsba.dxBarButton15Click(Sender: TObject);
var
  descri: string;
begin
 Ultima_Seleccion(Sender,1);
 EnvioCorreo.Vcarrera := SeleccionarCarrera('',descri);
 if (EnvioCorreo.Vcarrera='') then
  exit;
 Frmenviomail := TFrmenviomail.Create(Self);
 try
   Frmenviomail.Label4.Caption:= descri;
   EnvioCorreo.VUnidad := UnidadPrograma;
   EnvioCorreo.VUsuario := NombreUsuario;
   FrmEnvioMail.Position:=poScreenCenter;
   FrmEnvioMail.ShowModal;
 finally
   FrmEnvioMail.Free;
 end;
end;

procedure TFormEsba.FinalespormesasyComision1Click(Sender: TObject);
var
  carDes: string;
begin
 Ultima_Seleccion(Sender,1);

 FinalesxMesayComision.VCarrera        := SeleccionarCarrera('',carDes);
 if (FinalesxMesayComision.VCarrera='') then
   exit;
 FinalesxMesayComision.VUnidad         := UnidadPrograma;
 FinalesxMesayComision.VInstituto      := 'I. Estudios Superiores Bs. As.';
 FinalesxMesayComision.VCaracteristica := 'A-781';
 FinalesxMesayComision.VNombreUsuario  := NombreUsuario;
 Finalesmesacom := TFinalesmesacom.Create(Self);
 try
   Finalesmesacom.Position:=poScreenCenter;
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781 '+ carDes;
   Finalesmesacom.ShowModal;
 finally
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781';
   Finalesmesacom.Free;
 end;
end;

procedure TFormEsba.BtnBajasAltasClick(Sender: TObject);
begin
 { if FBajas='N' then begin
    FBajas := 'S';
  end
  else begin
    FBajas := 'N';
  end;
  AbroBDDAlumnosTHRead;}
end;

procedure TFormEsba.BtnBuscarClick(Sender: TObject);
begin
  if (TxtBusqueda.Text = '') Then begin
    ShowMessage('Escriba algo a buscar');
    exit;
  end;
  if (Length(TxtBusqueda.Text) < 2 ) Then begin
    ShowMessage('Al menos escriba 2 caracteres');
    exit;
  end;
  AbroBDDAlumnosTHRead;
end;

procedure TFormEsba.MnuFuenteClick(Sender: TObject);
begin
 If MenuContextual = 'GRILLA' Then
  Begin
   CambioFuente(Grilla.Font);
   Grilla.Color := MnuColorFondo.Color;
   Grilla.Refresh;
  End;
 If MenuContextual = 'MEMO' Then
  Begin
   CambioFuente(NotasUsuario.Font);
   NotasUsuario.Color := MnuColorFondo.Color;
   NotasUsuario.Refresh;
  End;
 MenuContextual := '';
end;

procedure TFormEsba.mnupermisosClick(Sender: TObject);
begin
FrmPermisosUsuario := TFrmPermisosUsuario.Create(Self);
try
                FrmPermisosUsuario.Position:=poScreenCenter;
                FrmPermisosUsuario.ShowModal;
finally
                FrmPermisosUsuario.Free;
end;

end;

procedure TFormEsba.mnuresetpassClick(Sender: TObject);
begin
Reseteo_pass;
end;

procedure TFormEsba.NotasUsuarioMouseUp(Sender: TObject; Button: TMouseButton;
  Shift: TShiftState; X, Y: Integer);
begin
 If Button = mbRight Then
  Begin
     MenuContextual := '';
     Mnufuente.Text :=  NotasUsuario.Font.Name;
     MnuColorFuente.Color := NotasUsuario.Font.Color;
     MnuTamanio.ItemIndex := NotasUsuario.Font.Size - 8;
     MnuColorFondo.Color := NotasUsuario.Color;
     MnuNegrita.Down   := FsBold In NotasUsuario.Font.Style;
     MnuCursiva.Down   := FsItalic In NotasUsuario.Font.Style;
     MnuSubrrayado.Down := FsUnderline In NotasUsuario.Font.Style;
     MenuContextual := 'MEMO';
     MnuContexFuente.PopupFromCursorPos;
  End;
end;

procedure TFormEsba.FechaMemoChange(Sender: TObject);
begin
   NotasUsuario.Lines.Insert(0,DateToStr(FechaMemo.Date));
end;

procedure TFormEsba.TxtBusquedaKeyPress(Sender: TObject; var Key: Char);
begin
  if (Key = #13) then
    BtnBuscarClick(BtnBuscar);
end;

procedure TFormEsba.GrillaKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
 Case Key Of
   VK_F2: TxtBusqueda.SetFocus;
   VK_F11: FormEsba.FormKeyUp(UltSel.Objeto, key, Shift);
 end;

End;

procedure TFormEsba.GrillaDrawColumnCell(Sender: TObject;
  const Rect: TRect; DataCol: Integer; Column: TColumn;
  State: TGridDrawState);
begin
 with TDBGrid(Sender) do begin
      if SelectedRows.IndexOf(DataSource.Dataset.Bookmark) >= 0 then begin
        {if (mtalumnos.fieldbyname('FCOLOR').Asinteger<>0) then
            Canvas.Brush.Color := mtalumnos.fieldbyname('FCOLOR').AsInteger
        else  }
            Canvas.Brush.Color := clPurple;
      end
      else if gdSelected in State then begin
        {if (mtalumnos.fieldbyname('FCOLOR').Asinteger<>0) then
            Canvas.Brush.Color := mtalumnos.fieldbyname('FCOLOR').Asinteger
        else }
            Canvas.Brush.Color := clHighlight;
            Canvas.Font.Color:=ClWhite;
      end else if (DataSource.Dataset.RecNo and 1) <> 0 then begin
        { if (mtalumnos.fieldbyname('FCOLOR').Asinteger<>0) then
            Canvas.Brush.Color := mtalumnos.fieldbyname('FCOLOR').Asinteger
         else }
            Canvas.Brush.Color := clWhite; //$00DDEEFF
            Canvas.Font.Color:=ClBlack;
      end
      else begin
       { if (mtalumnos.fieldbyname('FCOLOR').Asinteger<>0) then
            Canvas.Brush.Color := mtalumnos.fieldbyname('FCOLOR').Asinteger
         else}
        Canvas.Brush.Color := clCream; //$00DDFFFF;
      end;
      if (mtalumnos.fieldbyname('FCOLOR').Asinteger<>0) then
         Canvas.Brush.Color:=mtalumnos.fieldbyname('FCOLOR').Asinteger;
      DefaultDrawColumnCell(Rect,DataCol,Column,State);
    end;
end;

procedure TFormEsba.VerNotasClick(Sender: TObject);
begin
  If Panel1.Visible Then
   Begin
     Panel1.Visible:=False;
     NotasUsuario.Visible:=False;
     VerNotas.Caption:='Notas Personales';
     //Panel1.Align:=Alnone;
  //   notasUsuario.Align:=Alnone;
   End
  Else
   Begin
     NotasUsuario.Visible:=True;
     Panel1.Visible:=True;
//     Panel1.Align:=AlBottom;
//     notasUsuario.Align:=AlBottom;
     VerNotas.Caption:='Ocultar Notas';
   End
end;

procedure TFormEsba.ListadoDinamico(Sender: TObject);
begin
  If ( (sender is TdxBarButton) and (FileExists(PathListados+CARPETA_LISTADOS+'\'+TdxBarButton(Sender).Caption+'.lst')))
     or
     ( (sender is TdxNavBarItemLink) and (FileExists(PathListados+CARPETA_LISTADOS+'\'+TdxNavBarItemLink(Sender).Item.Caption+'.lst')))
     then
   Begin
    Ultima_Seleccion(Sender,1);
    if Sender is TdxBarButton then
        moduloVariable.Archivo:= PathListados+CARPETA_LISTADOS+'\'+TdxBarButton(Sender).Caption+'.lst'
    else if Sender is TdxNavBarItemLink then
        moduloVariable.Archivo:= PathListados+CARPETA_LISTADOS+'\'+TdxNavBarItemLink(Sender).Item.Caption+'.lst';

    modulovariable.Unidad:=UnidadPrograma;
    FrmModVariable := TFrmModVariable.Create(Self);
    FrmModVariable.Position:=poScreenCenter;
    If ModuloVariable.Cerrar Then begin
        if Sender is TdxBarButton then
            FrmModVariable.Caption:=TdxBarButton(Sender).Caption
        else if Sender is TdxNavBarItemLink then
            FrmModVariable.Caption:=TdxNavBarItemLink(Sender).Item.Caption;
        FrmModVariable.ShowModal;
    end;
    FrmModVariable.DestroyComponents;
    FrmModVariable.Free;
   End
  else
    Application.MessageBox('Listado no disponible','Aviso',MB_ICONINFORMATION);
end;

procedure TFormEsba.FormCreate(Sender: TObject);
Var GroupAux:String;
    x:Integer;
begin
  LblApellidoNombre.Caption:= '';
  LblCarrera.Caption:= '';

  iniciodesesion:=Tiniciodesesion.Create(self);
  IniciodeSesion.ShowModal;
  If Sesion.Pasa  Then Begin
        NombreUsuario := IniciodeSesion.nombre.Text;
        funcionesConfiguracion.NomUsu:= NombreUsuario;
        FuncionesConfiguracion.CodUsu := Sesion.codusu;
        FuncionesConfiguracion.superv := Sesion.superv;
        Seciones.SetCodSecion;
        With DataModule.CustomerData do Begin
              IbDsVarios.SQLs.SelectSQL.Clear;
              IbDsVarios.SQLs.SelectSQL.Text := 'SELECT CARRE, DESCARRE,RECTOR,DNIRECTOR,SECRETARIA,DNISECRET,DIRESTU, DNIDIRESTU, GRUPO, BAJA,IMAGEN, ESCARRE, TIPO, DISTANCIA, REGSOLAPA, MNUOPC FROM YYY_BARRA_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+',''ALUMNOS'') ORDER BY GRUPO, ORDEN;';
              TrDsVarios.Active := True;
              IbDsVarios.Active := True;
              funcionesConfiguracion.Rector:= IBDsVarios.FieldByName('RECTOR').Value;
              funcionesConfiguracion.Secretaria:=IBDsVarios.FieldByName('SECRETARIA').Value;
              funcionesConfiguracion.DniRector:=IBDsVarios.FieldByName('DNIRECTOR').Value;
              funcionesConfiguracion.DniSecre:=IBDsVarios.FieldByName('DNISECRET').Value;
              funcionesConfiguracion.DniDirEst:=IBDsVarios.FieldByName('DNIDIRESTU').Value;
              funcionesConfiguracion.DirEst:=IBDsVarios.FieldByName('DIRESTU').Value;
              IbDsVarios.FetchAll;
              SetLength(BarraCarrerasItems,IBDsVarios.RecordCount);
              x:=0;
              SetLength(BarraCarrerasGroup,x+1);
              GroupAux:=IBDsVarios.FieldByName('GRUPO').Value;

              While Not IbDsVarios.Eof Do Begin

                  BarraCarrerasItems[IBDsVarios.RecNo-1].Descripcion:=IBDsVarios.FieldByName('DESCARRE').Value;
                  BarraCarrerasItems[IBDsVarios.RecNo-1].CodCarre:=IBDsVarios.FieldByName('CARRE').Value;
                  BarraCarrerasItems[IBDsVarios.RecNo-1].Baja:=IBDsVarios.FieldByName('BAJA').Value;
                  BarraCarrerasItems[IBDsVarios.RecNo-1].EsCarre:=IBDsVarios.FieldByName('ESCARRE').Value;
                  BarraCarrerasItems[IBDsVarios.RecNo-1].tipo:= IBDsVarios.FieldByName('TIPO').Value;
                  BarraCarrerasItems[IBDsVarios.RecNo-1].regsolapa:= IBDsVarios.FieldByName('REGSOLAPA').Value;
                  if AnsiSameStr(IBDsVarios.FieldByName('DISTANCIA').AsString,'S') then
                    BarraCarrerasItems[IBDsVarios.RecNo-1].distancia:= true
                  else
                    BarraCarrerasItems[IBDsVarios.RecNo-1].distancia:= false;

                  if not IBDsVarios.FieldByName('MNUOPC').IsNull then
                    TDxBarButton(FormEsba.FindComponent(IBDsVarios.FieldByName('MNUOPC').AsString)).Enabled:=true;

                  IbDsVarios.Next;
              End;
              trDsVarios.Rollback;
              trDsVarios.Active := False;
        End;
  End
  Else begin
     Application.Terminate;
  end;
  iniciodesesion.Free;
  AsignaturaporEquivalencia1.OnClick:=ListadoDinamico;
  EquivalenciasInternas1.OnClick:=ListadoDinamico;
  InscriptosporComisionycuatrimestre1.OnClick:=ListadoDinamico;
  Completoporcuatrimestre1.OnClick:=ListadoDinamico;
  Nuevos1.OnClick:=ListadoDinamico;
  ReincorporacionporComMatCuat1.OnClick:=ListadoDinamico;
  LibresporComMatCuat1.OnClick:=ListadoDinamico;
  RecursantecompletoporCuat1.OnClick:=ListadoDinamico;
  PorCuatMatCondi1.OnClick:=ListadoDinamico;
  PorCuatCondicionCompleto1.OnClick:=ListadoDinamico;
  Queadeudenmasde2materiascompleto1.OnClick:=ListadoDinamico;
  Egresadosenrangodefechas1.OnClick:=ListadoDinamico;
  ProbablesegresadosporCuat1.OnClick:=ListadoDinamico;
  Comisionesarmadas1.OnClick:=ListadoDinamico;
  Profesoresporcomision1.OnClick:=ListadoDinamico;
//  Comisionesalmnisterio1.OnClick:=ListadoDinamico;
  Titulosylegalizaciones1.OnClick:=ListadoDinamico;
  DocumentacionporComision1.OnClick:=ListadoDinamico;
  DocumentacionGeneral1.OnClick:=ListadoDinamico;
  NoInscriptos.OnClick:=ListadoDinamico;
  ConsultadeFaltascomision.OnClick:=ListadoDinamico;
  ConsultadeFaltasalumno.OnClick:=ListadoDinamico;
  dxBarButton33.OnClick:=ListadoDinamico;
  dxBarButton35.OnClick:=ListadoDinamico;
  dxBarButton36.OnClick:=ListadoDinamico;
  dxBarButton38.OnClick:=ListadoDinamico; //listado de reincorporacion

  //VerNotasClick(sender);
end;

procedure TFormEsba.Carpetaasistencia1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);

  lstplanasis.VCarrera:=SeleccionarCarrera('N',NombreCarrera);
  if (lstplanasis.VCarrera = '') then
    exit;

  Frmlstplanasis:=TFrmlstplanasis.Create(self);
  try
    lstplanasis.VUnidad:=UnidadPrograma;
    lstplanasis.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lstplanasis.VCaracteristica:='A-781';
    lstplanasis.VNombreUsuario:=NombreUsuario;
    lstplanasis.VCarreraLarga:=NombreCarrera;
    Frmlstplanasis.Position:=poScreenCenter;
    Frmlstplanasis.ShowModal ;
  finally
    Frmlstplanasis.Free;
  end;
end;

procedure TFormEsba.Parciales1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  lstactasexamenes.VCarrera:=SeleccionarCarrera('',NombreCarrera);
  if (lstactasexamenes.VCarrera = '') then
    exit;
  Frmlstactas:=TFrmlstactas.Create(self);
  try
    lstactasexamenes.VUnidad:=UnidadPrograma;
    lstactasexamenes.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lstactasexamenes.VCaracteristica:='A-781';
    lstactasexamenes.VNombreUsuario:=NombreUsuario;
    lstactasexamenes.VCarreraLarga:=NombreCarrera;
    frmlstactas.Position:=poScreenCenter;
    Frmlstactas.ShowModal ;
  finally
    Frmlstactas.Free;
    Frmlstactas:=nil;
  end;
end;

procedure TFormEsba.Planillasdeprofesores1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  lstNotasyPractico.VCarrera:=SeleccionarCarrera('N',NombreCarrera);
  if (lstNotasyPractico.VCarrera = '') then
    exit;

  FrmPractNotas:=TFrmPractNotas.Create(self);
  try
    lstNotasyPractico.VUnidad:=UnidadPrograma;
    lstNotasyPractico.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lstNotasyPractico.VCaracteristica:='A-781';
    lstNotasyPractico.VNombreUsuario:=NombreUsuario;
    lstNotasyPractico.VCarreraLarga:=NombreCarrera;
    FrmPractNotas.Caption:='Impresion Planilla de calificacion';
    lstNotasyPractico.Plantilla:='Planilla_calificaciones.wmf';
    LstNotasyPractico.posi.inicio_enca:=2.3;
    LstNotasyPractico.posi.inicio_detalle:=6.8;
    LstNotasyPractico.posi.espaciado:=1.03;
    LstNotasyPractico.posi.corte:=31;
    LstNotasyPractico.posi.pos_orden:=0.75;
    LstNotasyPractico.posi.pos_nombre:=1.7;
    frmpractnotas.Position:=poScreenCenter;
    FrmPractNotas.ShowModal ;
  finally
    FrmPractNotas.Free;
  end;

end;

procedure TFormEsba.Carpetadetrabajos1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  lstNotasyPractico.VCarrera:=SeleccionarCarrera('N',NombreCarrera);
  if (lstNotasyPractico.VCarrera='') then
    exit;
  FrmPractNotas:=TFrmPractNotas.Create(self);
  try
    lstNotasyPractico.VUnidad:=UnidadPrograma;
    lstNotasyPractico.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lstNotasyPractico.VCaracteristica:='A-781';
    lstNotasyPractico.VNombreUsuario:=NombreUsuario;
    lstNotasyPractico.VCarreraLarga:=NombreCarrera;
    FrmPractNotas.Caption:='Impresion trabajos practicos';
    lstNotasyPractico.Plantilla:='trabajos_practicos.wmf';
    LstNotasyPractico.posi.inicio_enca:=2.3;
    LstNotasyPractico.posi.inicio_detalle:=6.8;
    LstNotasyPractico.posi.espaciado:=0.69;
    LstNotasyPractico.posi.corte:=26.5;
    LstNotasyPractico.posi.pos_orden:=0.85;
    LstNotasyPractico.posi.pos_nombre:=1.8;
    frmpractnotas.Position:=poScreenCenter;
    FrmPractNotas.ShowModal;
  finally
    FrmPractNotas.Free;
  end;

end;

procedure TFormEsba.Volantes1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
    lstactasMesas.VCarrera:=SeleccionarCarrera('',NombreCarrera);
  if (lstactasMesas.VCarrera='') then
    exit;
  FrmlstMesas:=TFrmlstMesas.Create(self);
  try
    lstactasMesas.VUnidad:=UnidadPrograma;
    lstactasMesas.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lstactasMesas.VCaracteristica:='A-781';
    lstactasMesas.VNombreUsuario:=NombreUsuario;
    lstactasMesas.VCarreraLarga:=NombreCarrera;
    frmlstmesas.Position:=poScreenCenter;
    FrmlstMesas.ShowModal;
  finally
    FrmlstMesas.Free;
  end;
end;

procedure TFormEsba.Reincorporacion1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  lstactasReincorporacion.VCarrera:=SeleccionarCarrera('N',NombreCarrera);
  if (lstactasReincorporacion.VCarrera='') then
    exit;
  FrmlstReincorporacion:=TFrmlstReincorporacion.Create(self);
  try
    lstactasReincorporacion.VUnidad:=UnidadPrograma;
    lstactasReincorporacion.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lstactasReincorporacion.VCaracteristica:='A-781';
    lstactasReincorporacion.VNombreUsuario:=NombreUsuario;
    lstactasReincorporacion.VCarreraLarga:=NombreCarrera;
    frmlstreincorporacion.Position:=poScreenCenter;
    FrmlstReincorporacion.ShowModal;
  finally
    FrmlstReincorporacion.Free;
  end;

end;

procedure TFormEsba.ARegular1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
    lstactasARegular.VCarrera:=SeleccionarCarrera('N',NombreCarrera);
  if (  lstactasARegular.VCarrera='') then
    exit;
  FrmlstARegular:=TFrmlstARegular.Create(self);
  try
    lstactasARegular.VUnidad:=UnidadPrograma;
    lstactasARegular.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lstactasARegular.VCaracteristica:='A-781';
    lstactasARegular.VNombreUsuario:=NombreUsuario;
    lstactasARegular.VCarreraLarga:=NombreCarrera;
    frmlstaregular.Position:=poScreenCenter;
    FrmlstARegular.ShowModal;
  finally
    FrmlstARegular.Free;
  end;
end;

procedure TFormEsba.ApplicationEvents1Message(var Msg: tagMSG;
  var Handled: Boolean);
var
   i: SmallInt;
begin
   if (Msg.message = WM_MOUSEWHEEL) then
   begin
     Msg.message := WM_KEYDOWN;
     Msg.lParam := 0;
     i := HiWord(Msg.wParam) ;
     if i > 0 then
       Msg.wParam := VK_UP
     else
       Msg.wParam := VK_DOWN;

     Handled := False;
   end;
end;

procedure TFormEsba.dxBarButton17Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  FrmImpresionEqTerc:=TFrmImpresionEqTerc.Create(self);
  try
    lst_impresion_equivalencia_terc.VUnidad:=UnidadPrograma;
    lst_impresion_equivalencia_terc.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lst_impresion_equivalencia_terc.VCaracteristica:='A-781';
    lst_impresion_equivalencia_terc.VNombreUsuario:=NombreUsuario;
    lst_impresion_equivalencia_terc.VCarreraLarga:= MTAlumnos.FieldByName('CARRERASC').AsString;
    lst_impresion_equivalencia_terc.VCodAlu := MTAlumnos.FieldByName('COD_ALU').AsString;
    lst_impresion_equivalencia_terc.VCarrera:= MTAlumnos.FieldByName('CODCARRE').AsString;
    FrmImpresionEqTerc.Position:=poScreenCenter;
    FrmImpresionEqTerc.ShowModal;
  finally
    FrmImpresionEqTerc.Free;
  end;

end;

procedure TFormEsba.dxBarButton7Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  FrmImpresionEqBac:=TFrmImpresionEqBac.Create(self);
  try
    lst_impresion_equivalencia_bac.VUnidad:=UnidadPrograma;
    lst_impresion_equivalencia_bac.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    lst_impresion_equivalencia_bac.VCaracteristica:='A-781';
    lst_impresion_equivalencia_bac.VNombreUsuario:=NombreUsuario;
    lst_impresion_equivalencia_bac.VCarreraLarga:=NombreCarrera;
    lst_impresion_equivalencia_bac.VCodAlu := MTAlumnos.FieldByName('COD_ALU').AsString;
    lst_impresion_equivalencia_bac.VCarrera:= MTAlumnos.FieldByName('CODCARRE').AsString;
    frmimpresioneqbac.Position:=poScreenCenter;
    FrmImpresionEqbac.ShowModal;
  finally
    FrmImpresionEqbac.Free;
  end;
end;


procedure TFormEsba.FormKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (key=VK_F11) then
     Ultima_Seleccion(UltSel.Objeto,2);
end;

procedure TFormEsba.FormShow(Sender: TObject);
var x, pos:Integer;
begin
     BarradeEstado.Panels.Items[Panel_dia].Text := DateToStr(DATE);
     BarradeEstado.Panels.Items[Panel_Usuario].Text := NombreUsuario;
     BarradeEstado.Panels.Items[Panel_institucion].Text := 'Instituto de Estudios Superiores de Buenos Aires A-781';
     BarradeEstado.Panels.Items[Panel_Path].Text := UnidadPrograma;
     CargoConfiguracion(NombreUsuario);
end;

procedure TFormEsba.Comisionesalmnisterio1Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  ComisionesAlMinisterio.VCarrera:=SeleccionarCarrera('N',NombreCarrera);
  if (ComisionesAlMinisterio.VCarrera='') then
    exit;
  FrmComAlMinisterio:=TFrmComAlMinisterio.Create(self);
  try
    ComisionesAlMinisterio.VUnidad:=UnidadPrograma;
    ComisionesAlMinisterio.VInstituto:='Instituto de Estudios Superiores de Buenos Aires';
    ComisionesAlMinisterio.VCaracteristica:='A-781';
    ComisionesAlMinisterio.VNombreUsuario:=NombreUsuario;
    ComisionesAlMinisterio.VCarreraLarga:=NombreCarrera;
    ComisionesAlMinisterio.UnidadPrograma:=UnidadPrograma;
    FrmComAlMinisterio.Position:=poScreenCenter;
    FrmComAlMinisterio.ShowModal;
  finally
    FrmComAlMinisterio.Free;
  end;

end;

procedure TFormEsba.dxBarButton18Click(Sender: TObject);
var
  carDes: string;
begin
 Ultima_Seleccion(Sender,1);
 CargadePermisosMasivo.VCarrera := SeleccionarCarrera('', carDes);
 if (CargadePermisosMasivo.VCarrera='') then
  exit;
 FrmCargaPermisos := TFrmCargaPermisos.Create(Self);
 try
   CargadePermisosMasivo.VUnidad := UnidadPrograma;
   CargadePermisosMasivo.VInstituto := 'I. Estudios Superiores Bs. As.';
   CargadePermisosMasivo.VCaracteristica := 'A-781';
   CargadePermisosMasivo.VNombreUsuario := NombreUsuario;
   FrmCargaPermisos.Position:=poScreenCenter;
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781 '+ carDes;
   FrmCargaPermisos.ShowModal;
 finally
   FormEsba.Caption:= 'Estudios Superiores de Buenos Aires A-781';
   FrmCargaPermisos.Free;
 end;
end;

procedure TFormEsba.dxBarButton19Click(Sender: TObject);
var
  msg:string;
begin
  Ultima_Seleccion(Sender,1);
  Parametros.Carrera_activa:=SeleccionarCarrera('N');
  if (Parametros.Carrera_activa='') then
    exit;
  FrmParametros := TFrmParametros.Create(Self);
  Try
      FrmParametros.Memo:=Tmemo.Create(FrmParametros);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=FrmParametros;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.Lines.Clear;
      if (Parametros.Carrera_activa='333') or (Parametros.Carrera_activa='650') then
        FrmParametros.Memo.Lines.Text:='Comision;COMI;N;S;'+#13+'Fecha;Fecha;D;S;'//+#13+'Cuatrimestre;CUAT;N;S;&CUA_ACT'
      else
        FrmParametros.Memo.Lines.Text:='Comision;COMI;N;S;'+#13+
                                       'Fecha;Fecha;D;S;'+#13+
                                       'Cuatrimestre;CUAT;N;S;&CUA_ACT'+#13+
                                       'Materia;MAT;K;S;"SELECT CODMATERI,DESCRIPCI FROM MATERIAS WHERE CODCARRE='''+Parametros.Carrera_activa+'''";"Codigo,Nombre";0;CODMATERI';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.Refresh;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    CargaInasistenciasComision.fecha:=TDateEdit(FrmParametros.LstParam[1].control).Date;
    CargaInasistenciasComision.comi:=TCurrencyEdit(FrmParametros.LstParam[0].control).Text;
    if (Parametros.Carrera_activa='333') or (Parametros.Carrera_activa='650') then begin
      CargaInasistenciasComision.materia:='';

      CargaInasistenciasComision.cuaanio:='1'+Copy(IntToStr(YearOf(CargaInasistenciasComision.fecha)),3,2);
    end
    else begin
      CargaInasistenciasComision.materia:=TComboEdit(FrmParametros.LstParam[3].control).Text;
      CargaInasistenciasComision.cuaanio:=TCurrencyEdit(FrmParametros.LstParam[2].control).Text;
    end;

    FrmParametros.DestroyComponents;
    FrmParametros.Free;
    FrmCargaInaComi := TFrmCargaInaComi.Create(Self);
    CargaInasistenciasComision.VCarrera := Parametros.Carrera_activa;
    CargaInasistenciasComision.VUnidad := UnidadPrograma;
    CargaInasistenciasComision.VInstituto := 'I. Estudios Superiores Bs. As.';
    CargaInasistenciasComision.VCaracteristica := 'A-781';
    CargaInasistenciasComision.VNombreUsuario := NombreUsuario;
    FrmCargaInaComi.Position:=poScreenCenter;
    msg:=FrmCargaInaComi.ValidateDatosParametros;
    if msg='' then
      FrmCargaInaComi.ShowModal
    else
      ShowMessage(msg);
    FrmCargaInaComi.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton20Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:=Carrera_activa;
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Docen. Desde";DOCDDE;K;N;"SELECT CODPROFES,DOCENTE FROM DOCENTES WHERE FECHA_BAJ IS NULL";"Codigo,Nombre";0;CODPROFES;'+#13+
                                     '"Docen. Hasta";DOCHTA;K;N;"SELECT CODPROFES,DOCENTE FROM DOCENTES WHERE FECHA_BAJ IS NULL";"Codigo,Nombre";0;CODPROFES;'+#13+
                                     '"Fecha desde";FECDDE;D;S;'+#13+
                                     '"Fecha hasta";FECHTA;D;S;'+#13+
                                     'Carrera;CARRE;K;NM;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT'+#13+
                                     'Firma;FIRMA;L;S;"Rector/a,Secretaria,Dir.Estud.";"R,S,D";0'+#13+
                                     '"Imagen Firma";IMGFIRMA;L;S;"Si,No";"S,N";0';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    Impresiones.UnidadPrograma := UnidadPrograma;
    Impresiones.Imp_Mesas_citacion(TComboEdit(FrmParametros.LstParam[0].control).Text,TComboEdit(FrmParametros.LstParam[1].control).Text,FormatDateTime('mm/dd/yyyy',TDateEdit(FrmParametros.LstParam[2].control).Date),FormatDateTime('mm/dd/yyyy',TDateEdit(FrmParametros.LstParam[3].control).Date),TComboEdit(FrmParametros.LstParam[4].control).Text,UnidadPrograma,FrmParametros.LstParam[5].Valores_lista[TComboBox(FrmParametros.LstParam[5].control).ItemIndex],FrmParametros.LstParam[6].Valores_lista[TComboBox(FrmParametros.LstParam[6].control).ItemIndex]);
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;

end;

procedure TFormEsba.dxBarButton21Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:=Carrera_activa;
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='Carrera;CARRE;K;N;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT'+#13+
                                     '"Fecha desde";FECDDE;D;S;'+#13+
                                     '"Fecha hasta";FECHTA;D;S;';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    Impresiones.Imp_Mesas_ParteDiario(FormatDateTime('mm/dd/yyyy',TDateEdit(FrmParametros.LstParam[1].control).Date),FormatDateTime('mm/dd/yyyy',TDateEdit(FrmParametros.LstParam[2].control).Date),TComboEdit(FrmParametros.LstParam[0].control).Text,UnidadPrograma);
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;

end;

procedure TFormEsba.dxBarButton22Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  CargadeTrimestres.modo:='TRIMESTRAL';
  FrmCargaTri:=TFrmCargaTri.Create(self);
  FrmCargaTri.Caption:='Carga de trimestres uso 333';
  FrmCargatri.Position:=poScreenCenter;
  FrmCargatri.ShowModal;
  FrmCargatri.Free;
  FrmCargaTri:=Nil;
end;

procedure TFormEsba.dxBarButton23Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  Parametros.Carrera_activa:='';

  FrmParametros := TFrmParametros.Create(Self);
  Try

      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Carrera";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT'+#13+
                                     'Comision;CUTUCO;N;N;'+#13+
                                     'Cuatrimestre;CUAT;N;S;&CUA_ACT';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    FrmConsulRein:=TFrmConsulRein.Create(self);
    ConsultaReincorporaciones.Carrera:=TComboEdit(FrmParametros.LstParam[0].control).Text;
    ConsultaReincorporaciones.Cutuco:=TCurrencyEdit(FrmParametros.LstParam[1].control).Text;
    ConsultaReincorporaciones.Cuat:=TCurrencyEdit(FrmParametros.LstParam[2].control).Text;
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
    ConsultaReincorporaciones.VCarrera := NombreCarrera;
    ConsultaReincorporaciones.VUnidad := UnidadPrograma;
    ConsultaReincorporaciones.VInstituto := 'I. Estudios Superiores Bs. As.';
    ConsultaReincorporaciones.VCaracteristica := 'A-781';
    ConsultaReincorporaciones.VNombreUsuario := NombreUsuario;
    FrmConsulRein.Position:=poScreenCenter;
    FrmConsulRein.ShowModal;
    FrmConsulRein.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;

end;

procedure TFormEsba.dxBarButton24Click(Sender: TObject);
var
  fbajas: string;
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:='';
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Carrera";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT'+#13+
                                     'Comision;CUTUCO;N;N;'+#13+
                                     'Año;ANIO;N;S;'+#13+
                                     'Imp.Sanciones;IMPS;L;S;Si,No;S,N;0'+#13+
                                     'Bajas;BAJAS;L;S;Si,No;S,N;1';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    if (TComboBox(FrmParametros.LstParam[4].Control).ItemIndex=0) then
      fbajas:='S'
    else
      fbajas:='N';
    Impresiones.Imp_Boletin_calif(TComboEdit(FrmParametros.LstParam[0].control).Text,'',UnidadPrograma,Fbajas,StrToInt(TCurrencyEdit(FrmParametros.LstParam[2].control).Text),trunc(TCurrencyEdit(FrmParametros.LstParam[1].control).Value),TComboBox(FrmParametros.LstParam[3].Control).ItemIndex);
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton25Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:=MTAlumnos.FieldByName('CODCARRE').AsString;
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Año Carrera";ANIO;K;S;"SELECT DISTINCT CUATRIM FROM MATERIAS M WHERE M.CODCARRE='+#39+MTAlumnos.FieldByName('CODCARRE').AsString+#39+'";"Año";;CUATRIM;'+#13+
                                     'Imp.Sanciones;IMPS;L;S;Si,No;S,N;0';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    Impresiones.Imp_Boletin_calif(MTAlumnos.FieldByName('CODCARRE').AsString,MTAlumnos.FieldByName('COD_ALU').AsString,UnidadPrograma,MTAlumnos.FieldByName('BAJA').AsString,StrToInt(TComboEdit(FrmParametros.LstParam[0].control).Text),0,TComboBox(FrmParametros.LstParam[1].Control).ItemIndex);
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton26Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:=MTAlumnos.FieldByName('CODCARRE').AsString;
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Año Carrera";ANIO;K;S;"SELECT DISTINCT CUATRIM FROM MATERIAS M WHERE M.CODCARRE='+#39+MTAlumnos.FieldByName('CODCARRE').AsString+#39+'";"Año";;CUATRIM';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    Impresiones.Imp_Boletin_Inasistencias(MTAlumnos.FieldByName('CODCARRE').AsString,MTAlumnos.FieldByName('COD_ALU').AsString,UnidadPrograma,MTAlumnos.FieldByName('BAJA').AsString,StrToInt(TComboEdit(FrmParametros.LstParam[0].control).Text),0);
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton27Click(Sender: TObject);
var
  fbajas:string;
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:='';
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Carrera";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT'+#13+
                                     'Comision;CUTUCO;N;N;'+#13+
                                     'Año;ANIO;N;S;'#13+
                                     'Bajas;BAJAS;L;S;Si,No;S,N;1';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin

    if (TComboBox(FrmParametros.LstParam[3].Control).ItemIndex=0) then
      fbajas:='S'
    else
      fbajas:='N';
    Impresiones.Imp_Boletin_Inasistencias(TCurrencyEdit(FrmParametros.LstParam[0].control).Text,'',UnidadPrograma,Fbajas,StrToInt(TCurrencyEdit(FrmParametros.LstParam[2].control).Text),trunc(TCurrencyEdit(FrmParametros.LstParam[1].control).Value));
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton28Click(Sender: TObject);
begin
  Ultima_Seleccion(Sender,1);
  try
    CargadeTrimestres.modo:='CUATRIMESTRAL';
    FrmCargaTri:=TFrmCargaTri.Create(self);
    FrmCargaTri.Caption:='Carga de Cuatrimestres (Para carreras Cuatrimestrales)';
    FrmCargatri.Position:=poScreenCenter;
    FrmCargatri.ShowModal;
  finally
    FrmCargatri.Free;
    FrmCargaTri:=Nil;
  end;
end;

procedure TFormEsba.dxBarButton29Click(Sender: TObject);
begin
      FrmParametros := TFrmParametros.Create(Self);
      Try
          Parametros.Carrera_activa:='';
          FrmParametros.Memo:=TMemo.Create(Self);
          FrmParametros.Memo.Width:=1;
          FrmParametros.Memo.Height:=1;
          FrmParametros.Memo.Visible:=False;
          FrmParametros.Memo.ScrollBars:=ssBoth;
          FrmParametros.Memo.Parent:=Self;
          FrmParametros.Memo.Lines.Clear;
          FrmParametros.Memo.Lines.Text:='"Car.Destino";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT';
          Parametros.FrmParametros.CargoRegistro;
          Parametros.Pasa:=True;
          FrmParametros.ShowModal;
      except
          Parametros.Pasa:=False;
      end;
      if Parametros.Pasa Then begin
         CustomerData.IBUpVarios.Sql.Text:='SELECT FERRCOD, FERRMSG FROM XXX_COPIA_ALUMNO('+#39+MTAlumnos.FieldByName('CODCARRE').AsString+#39+','+#39+TComboEdit(FrmParametros.LstParam[0].control).Text+#39+','+#39+MTAlumnos.FieldByName('COD_ALU').AsString+#39+','+IntToStr(FuncionesConfiguracion.CodUsu)+')';
         CustomerData.TrUpVarios.Active:=True;
         CustomerData.IBUpVarios.ExecQuery;
         If (CustomerData.IBupVarios.FieldByName('FERRCOD').AsInteger=2) Then Begin
                MessageDlg(PChar(NombreUsuario + ', '+CustomerData.IBUpVarios.FieldByName('FERRMSG').AsString),mtError,[mbok],0,mbok);
                CustomerData.TRUpVarios.Rollback;
                CustomerData.TRUpVarios.Active := False;
                CustomerData.IBUpVarios.Close;
         End
         ELSE If (CustomerData.IBUpVarios.FieldByName('FERRCOD').AsInteger=1) and
                 (MessageDlg(PChar(NombreUsuario + ', '+CustomerData.IBUpVarios.FieldByName('FERRMSG').AsString),mtConfirmation,[mbyes,mbno],0,mbyes)=mryes) then begin
                  CustomerData.TRUpVarios.Commit;
                  CustomerData.TRUpVarios.Active := False;
                  CustomerData.IBUpVarios.Close;
         End
         else If (CustomerData.IBUpVarios.FieldByName('FERRCOD').AsInteger=0) then begin
                  CustomerData.TRUpVarios.Commit;
                  CustomerData.TRUpVarios.Active := False;
                  CustomerData.IBUpVarios.Close;
         end
         else begin
                CustomerData.TRUpVarios.Rollback;
                CustomerData.TRUpVarios.Active := False;
                CustomerData.IBUpVarios.Close;
         end;
        FrmParametros.DestroyComponents;
        FrmParametros.Free;
      end
      else begin
         FrmParametros.DestroyComponents;
         FrmParametros.Free;
      end;
end;

procedure TFormEsba.dxBarButton30Click(Sender: TObject);
begin
    FrmParametros := TFrmParametros.Create(Self);
      Try
          Parametros.Carrera_activa:='';
          FrmParametros.Memo:=TMemo.Create(Self);
          FrmParametros.Memo.Width:=1;
          FrmParametros.Memo.Height:=1;
          FrmParametros.Memo.Visible:=False;
          FrmParametros.Memo.ScrollBars:=ssBoth;
          FrmParametros.Memo.Parent:=Self;
          FrmParametros.Memo.Lines.Clear;
          FrmParametros.Memo.Lines.Text:='"Car.Destino";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT';
          Parametros.FrmParametros.CargoRegistro;
          Parametros.Pasa:=True;
          FrmParametros.ShowModal;
      except
          Parametros.Pasa:=False;
      end;
      if Parametros.Pasa Then begin
         CustomerData.IBUpVarios.Sql.Text:='SELECT FERRCOD, FERRMSG FROM XXX_MUEVE_ALUMNO('+#39+MTAlumnos.FieldByName('CODCARRE').AsString+#39+','+#39+TComboEdit(FrmParametros.LstParam[0].control).Text+#39+','+#39+MTAlumnos.FieldByName('COD_ALU').AsString+#39+','+IntToStr(FuncionesConfiguracion.CodUsu)+')';
         CustomerData.TrUpVarios.Active:=True;
         CustomerData.IBUpVarios.ExecQuery;
         If (CustomerData.IBupVarios.FieldByName('FERRCOD').AsInteger=2) Then Begin
                MessageDlg(PChar(NombreUsuario + ', '+CustomerData.IBUpVarios.FieldByName('FERRMSG').AsString),mtError,[mbok],0,mbok);
                CustomerData.TRUpVarios.Rollback;
                CustomerData.TRUpVarios.Active := False;
                CustomerData.IBUpVarios.Close;
         End
         ELSE If (CustomerData.IBUpVarios.FieldByName('FERRCOD').AsInteger=1) and
                 (MessageDlg(PChar(NombreUsuario + ', '+CustomerData.IBUpVarios.FieldByName('FERRMSG').AsString),mtConfirmation,[mbyes,mbno],0,mbyes)=mryes) then begin
                  CustomerData.TRUpVarios.Commit;
                  CustomerData.TRUpVarios.Active := False;
                  CustomerData.IBUpVarios.Close;
         End
         else If (CustomerData.IBUpVarios.FieldByName('FERRCOD').AsInteger=0) then begin
                  CustomerData.TRUpVarios.Commit;
                  CustomerData.TRUpVarios.Active := False;
                  CustomerData.IBUpVarios.Close;
         end
         else begin
                CustomerData.TRUpVarios.Rollback;
                CustomerData.TRUpVarios.Active := False;
                CustomerData.IBUpVarios.Close;
         end;
        FrmParametros.DestroyComponents;
        FrmParametros.Free;
      end
      else begin
         FrmParametros.DestroyComponents;
         FrmParametros.Free;
      end;
end;

procedure TFormEsba.dxBarButton31Click(Sender: TObject);
Var
  Nombre:Variant;
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:='';
      FrmParametros.Memo:=Tmemo.Create(FrmParametros);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=FrmParametros;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.Lines.Clear;
      {if (CarreraActiva='333') or (CarreraActiva='650') then
        FrmParametros.Memo.Lines.Text:='Carrera;CARRE;K;S;"SELECT CARRE, DESCARRE FROM CARRERA";"Codigo,Nombre";;CARRE;&CAR_ACT'#13+
                                       'Desde;DDE;D;S;'+#13+
                                       'Hasta;HTA;D;S;'+#13+
                                       'Alumno;ALUMNO;K;S;"SELECT COD_ALU, NOM_APE, APELLIDO FROM ALUMNOS WHERE CARRE=&CARRE";"Codigo,Nombre,Apellido";;COD_ALU;'
      else}
        FrmParametros.Memo.Lines.Text:='Carrera;CARRE;K;S;"SELECT CARRE, DESCARRE FROM CARRERA";"Codigo,Nombre";;CARRE;&CAR_ACT'#13+
                                       'Desde;DDE;D;S;'+#13+
                                       'Hasta;HTA;D;S;'+#13+
                                       'Alumno;ALUMNO;K;S;"SELECT COD_ALU, NOM_APE, APELLIDO FROM ALUMNOS WHERE CARRE=&CARRE";"Codigo,Nombre,Apellido";;COD_ALU;'+#13+
                                       'Materia;MAT;K;S;"SELECT CODMATERI,DESCRIPCI FROM MATERIAS WHERE CODCARRE=&CARRE";"Codigo,Nombre";0;CODMATERI';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.Refresh;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin

    ModificacionInasistenciasComision.dde:=FormatDateTime('mm/dd/yyyy',TDateEdit(FrmParametros.LstParam[1].control).Date);
    ModificacionInasistenciasComision.hta:=FormatDateTime('mm/dd/yyyy',TDateEdit(FrmParametros.LstParam[2].control).Date);
    ModificacionInasistenciasComision.Codalu:=TComboEdit(FrmParametros.LstParam[3].control).Text;
    ModificacionInasistenciasComision.VCarrera:=TComboEdit(FrmParametros.LstParam[0].control).Text;
    if (TComboEdit(FrmParametros.LstParam[0].control).Text='333') or (TComboEdit(FrmParametros.LstParam[0].control).Text='650') then
      ModificacionInasistenciasComision.materia:=''
    else
      ModificacionInasistenciasComision.materia:=TComboEdit(FrmParametros.LstParam[4].control).Text;

    FrmParametros.DestroyComponents;
    FrmParametros.Free;
    FrmModiInaComi := TFrmModiInaComi.Create(Self);
    ModificacionInasistenciasComision.VUnidad := UnidadPrograma;
    ModificacionInasistenciasComision.VInstituto := 'I. Estudios Superiores Bs. As.';
    ModificacionInasistenciasComision.VCaracteristica := 'A-781';
    ModificacionInasistenciasComision.VNombreUsuario := NombreUsuario;
    FrmModiInaComi.Position:=poScreenCenter;
    FuncionesDB.Consulta('SELECT APELLIDO||'', ''||NOM_APE FROM ALUMNOS WHERE COD_ALU='''+ModificacionInasistenciasComision.Codalu+''' AND CARRE='''+ModificacionInasistenciasComision.VCarrera+'''',nombre);
    FrmModiInaComi.Caption := 'Modificacion de Inasistencias '+VarToStr(nombre[0]);
    FrmModiInaComi.ShowModal;
    FrmModiInaComi.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton32Click(Sender: TObject);
var
  fbajas: string;
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:='';
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Carrera";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT'+#13+
                                     'Comision;CUTUCO;N;N;'+#13+
                                     'Cuatrimestre;CUAT;N;S;'+#13+
                                     'Estado;ESTADO;L;S;"Todos,No_Recursantes,Resursantes";"T,N,R";0'+#13+
                                     'Bajas;BAJAS;L;S;Si,No;S,N;1';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin

    if (TComboBox(FrmParametros.LstParam[4].Control).ItemIndex=0) then
      fbajas:='S'
    else
      fbajas:='N';
    Impresiones.Imp_Calificador(TComboEdit(FrmParametros.LstParam[0].control).Text,'',UnidadPrograma,Fbajas,StrToInt(TCurrencyEdit(FrmParametros.LstParam[2].control).Text),trunc(TCurrencyEdit(FrmParametros.LstParam[1].control).Value),FrmParametros.LstParam[3].Valores_lista[TComboBox(FrmParametros.LstParam[3].control).ItemIndex]);
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton34Click(Sender: TObject);
var
  fbajas: string;
begin
  Ultima_Seleccion(Sender,1);
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:='';
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      FrmParametros.Memo.Lines.Text:='"Carrera";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT'+#13+
                                     'Comision;CUTUCO;N;N;'+#13+
                                     'Cuatrimestre;CUAT;N;S;'#13+
                                     'Bajas;BAJAS;L;S;Si,No;S,N;1';;
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin

    if (TComboBox(FrmParametros.LstParam[3].Control).ItemIndex=0) then
      fbajas:='S'
    else
      fbajas:='N';
    Impresiones.Imp_EtiqLegajo(TComboEdit(FrmParametros.LstParam[0].control).Text,'',UnidadPrograma,Fbajas,StrToInt(TCurrencyEdit(FrmParametros.LstParam[2].control).Text),trunc(TCurrencyEdit(FrmParametros.LstParam[1].control).Value));
    FrmParametros.DestroyComponents;
    FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

procedure TFormEsba.dxBarButton37Click(Sender: TObject);
var
  msg:string;
begin
  Ultima_Seleccion(Sender,1);
  Parametros.Carrera_activa:=SeleccionarCarrera('N');
  if (Parametros.Carrera_activa='') then
    exit;

  FrmParametros := TFrmParametros.Create(Self);
  Try
      FrmParametros.Memo:=Tmemo.Create(FrmParametros);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=FrmParametros;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.Lines.Clear;
      if (Parametros.Carrera_activa='333') or (Parametros.Carrera_activa='650') then
        FrmParametros.Memo.Lines.Text:='Comision;COMI;N;S;'+#13+
                                       'Cuatrimestre;CUAT;N;S;&CUA_ACT'
      else
        FrmParametros.Memo.Lines.Text:='Comision;COMI;N;S;'+#13+
                                       'Cuatrimestre;CUAT;N;S;&CUA_ACT'+#13+
                                       'Materia;MAT;K;S;"SELECT CODMATERI,DESCRIPCI FROM MATERIAS WHERE CODCARRE='''+Parametros.Carrera_activa+'''";"Codigo,Nombre";0;CODMATERI';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.Refresh;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
    CargaInasistenciasComisionNuevo.comi:=TCurrencyEdit(FrmParametros.LstParam[0].control).Text;
    CargaInasistenciasComisionNuevo.cuaanio:=TCurrencyEdit(FrmParametros.LstParam[1].control).Text;
    if (Parametros.Carrera_activa='333') or (Parametros.Carrera_activa='650') then
      CargaInasistenciasComisionNuevo.materia:=''
    else
      CargaInasistenciasComisionNuevo.materia:=TComboEdit(FrmParametros.LstParam[2].control).Text;

    FrmParametros.DestroyComponents;
    FrmParametros.Free;
    FrmCargaInaComiNuevo := TFrmCargaInaComiNuevo.Create(Self);
    CargaInasistenciasComisionNuevo.VCarrera := Parametros.Carrera_activa;
    CargaInasistenciasComisionNuevo.VUnidad := UnidadPrograma;
    CargaInasistenciasComisionNuevo.VInstituto := 'I. Estudios Superiores Bs. As.';
    CargaInasistenciasComisionNuevo.VCaracteristica := 'A-781';
    CargaInasistenciasComisionNuevo.VNombreUsuario := NombreUsuario;
    FrmCargaInaComiNuevo.Position:=poScreenCenter;
    msg:=FrmCargaInaComiNuevo.ValidateDatosParametros;
    if msg='' then
      FrmCargaInaComiNuevo.ShowModal
    else
      ShowMessage(msg);
    FrmCargaInaComiNuevo.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;
end;

Procedure TFormEsba.Reseteo_pass;
begin
      FrmParametros := TFrmParametros.Create(Self);
      Try
          Parametros.Carrera_activa:='';
          FrmParametros.Memo:=TMemo.Create(Self);
          FrmParametros.Memo.Width:=1;
          FrmParametros.Memo.Height:=1;
          FrmParametros.Memo.Visible:=False;
          FrmParametros.Memo.ScrollBars:=ssBoth;
          FrmParametros.Memo.Parent:=Self;
          FrmParametros.Memo.Lines.Clear;
          FrmParametros.Memo.Lines.Text:='"Usuario";USU;K;S;"SELECT CODUSU,NOMUSU FROM USUARIOS";"Codigo,Nombre";;CODUSU;';
          Parametros.FrmParametros.CargoRegistro;
          Parametros.Pasa:=True;
          FrmParametros.ShowModal;
      except
          Parametros.Pasa:=False;
      end;
      if Parametros.Pasa Then begin
         CustomerData.IBUpVarios.Sql.Text:='UPDATE USUARIOS SET PASSWD='+#39+'/'+#39+',CAMPASS=''S'' WHERE CODUSU='+TComboEdit(FrmParametros.LstParam[0].control).Text;
         CustomerData.TrUpVarios.Active:=True;
         CustomerData.IBUpVarios.ExecQuery;
         if CustomerData.IbupVArios.RowsAffected=0 then
            MessageDlg(PChar(NombreUsuario + ', no se pudo cambiar el password'),mtError,[mbok],0,mbok)
         else
            MessageDlg(PChar(NombreUsuario + ', se reseteo la clave a un punto'),mtError,[mbok],0,mbok);
         CustomerData.TrUpVarios.Commit;
         CustomerData.IBUpVarios.Close;
         FrmParametros.DestroyComponents;
         FrmParametros.Free;
      end
      else begin
         FrmParametros.DestroyComponents;
         FrmParametros.Free;
      end;
end;

function TFormEsba.SeleccionarCarrera(const aDesact:string; var aDesCarre: string):string;
var
  text: string;
  datos: variant;
begin
  text:= SeleccionarCarrera(aDesact);
  aDesCarre:='';
  if (text<>'') then begin
    FuncionesDB.Consulta('select descarre from carrera where carre='''+text+'''', datos);
    aDesCarre:= datos[0];
  end;

  result:= text;
end;

function TFormEsba.SeleccionarCarrera(const aDesact:string):string;
var
  text: string;
begin
  text:='';
  FrmParametros := TFrmParametros.Create(Self);
  Try
      Parametros.Carrera_activa:='';
      FrmParametros.Memo:=TMemo.Create(Self);
      FrmParametros.Memo.Width:=1;
      FrmParametros.Memo.Height:=1;
      FrmParametros.Memo.Visible:=False;
      FrmParametros.Memo.ScrollBars:=ssBoth;
      FrmParametros.Memo.Parent:=Self;
      FrmParametros.Memo.Lines.Clear;
      if (aDesact = 'N') then
         FrmParametros.Memo.Lines.Text:='"Carrera";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+') where desact=''N''";"Codigo,Nombre";;CARRE;&CAR_ACT'
      else
         FrmParametros.Memo.Lines.Text:='"Carrera";CARRE;K;S;"SELECT CARRE, DESCARRE FROM YYY_CARRE_SEGU('+IntToStr(FuncionesConfiguracion.CodUsu)+')";"Codigo,Nombre";;CARRE;&CAR_ACT';
      Parametros.FrmParametros.CargoRegistro;
      Parametros.Pasa:=True;
      FrmParametros.ShowModal;
  except
      Parametros.Pasa:=False;
  end;
  if Parametros.Pasa Then begin
     text:= TComboEdit(FrmParametros.LstParam[0].control).Text;
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end
  else begin
     FrmParametros.DestroyComponents;
     FrmParametros.Free;
  end;

  result:= text;
end;

end.

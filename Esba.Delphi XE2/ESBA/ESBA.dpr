program ESBA;

uses
  Forms,
  Windows,
  Messages,
  SysUtils,
  Classes,
  inifiles,
  Variants,
  FrmAltaAlumno in '.\Formulario Alta de alumno\FrmAltaAlumno.pas' {AltaAlumno},
  sesion in '.\Formulario de Inicio de Sesion\sesion.pas' {iniciodesesion},
  InscripcionDeMaterias in '.\Formulario Inscripcion de Materias (Sirve)\InscripcionDeMaterias.pas' {InscripcionMaterias},
  constanciaalumnos in '.\Formulario  constancia alumno (Sirve)\constanciaalumnos.pas' {ConstanciadelAlumno},
  constanciaalumnoregular in '.\Formulario Constancia Alumno Regular\constanciaalumnoregular.pas' {ConstAlumnoRegular},
  ElimProfes in '.\Eliminacion de Profesores\ElimProfes.pas' {EliminacionProfes},
  Imprimir in '.\Formulario Impresion\Imprimir.pas' {FormPreview},
  altamodifmaterias in '.\Formulario Alta de materias\altamodifmaterias.pas' {AlMoMaterias},
  MesasExamen in '.\Formulario Alta-Modificacion-Baja Mesas de Examen\MesasExamen.pas' {mesasexamenes},
  PermisoExamen in '.\Formulario Permisos de Examenes\PermisoExamen.pas' {PermisosExamen},
  NotasExamenFinal in '.\Formulario Notas de examenes finales\NotasExamenFinal.pas' {NotasExamenes},
  Equivalencia in '.\Formulario Equivalencias\Equivalencia.pas' {FRMEquivalencias},
  CambioCodAlu_LM in '.\Formulario de Cambio de Cod_Alu y LM\CambioCodAlu_LM.pas' {ModifCodAlu_LM},
  ModifAnalitico in '.\Formulario alta-modificacion de analitico\ModifAnalitico.pas' {AMAnalitico},
  FrmAltaModProfes in '.\Alta - Modificacion de profesores Hecho por 2º vez\FrmAltaModProfes.pas' {FrmAltaModifProf},
  cargacomisiones in '.\formulario carga de comisiones\cargacomisiones.pas' {FrmComArmadas},
  CertServ in '.\Formulario Certificacion de Servisios\CertServ.pas' {FrmCertServ},
  AltaUsuario in '.\Formulario Alta de Usuarios\AltaUsuario.pas' {FrmAltaUsuario},
  enviocorreo in '.\Formulario de envio de correo por comision\enviocorreo.pas' {Frmenviomail},
  FinalesxMesayComision in '.\Formulario finales  por mesa y comison\FinalesxMesayComision.pas' {Finalesmesacom},
  BajaUsuarios in '.\Formulario baja de usuarios\BajaUsuarios.pas' {FrmBajaUsuarios},
  FrmEsba in 'FrmEsba.pas' {FormEsba},
  CnfMail in '.\Formulario de configuracion de correo\CnfMail.pas' {FrmCnfMail},
  DataModule in 'DataModule.pas' {CustomerData: TDataModule},
  uMensajesError in 'uMensajesError.pas',
  parametros in '.\Modulo Variable\parametros.pas' {FrmParametros},
  Busqueda in '.\Modulo Variable\Busqueda.pas' {FrmBusqueda},
  lstplanasis in '.\Formulario Listado de Asistencias\lstplanasis.pas' {Frmlstplanasis},
  lstactasexamenes in '.\Formulario Listado de actas de examenes\lstactasexamenes.pas' {Frmlstactas},
  lstNotasyPractico in '.\Formulario listado notas y practico\lstNotasyPractico.pas' {FrmPractNotas},
  modulovariable in '.\Modulo Variable\modulovariable.pas' {FrmModVariable},
  lstactasreincorporacion in '.\Formulario Listado de actas de reincorporacion\lstactasreincorporacion.pas' {FrmlstReincorporacion},
  lstactasARegular in '.\Formulario Listado de actas de A-REGULAR\lstactasARegular.pas' {FrmlstARegular},
  lstactasMesas in '.\Formulario Listado de actas de Mesas de Examenes\lstactasMesas.pas' {FrmlstMesas},
  lst_impresion_equivalencia_terc in '.\Formulario impresion equivalencia terciaria\lst_impresion_equivalencia_terc.pas' {FrmImpresionEqTerc},
  lst_impresion_equivalencia_bac in '.\Formulario impresion equivalencia bachiller\lst_impresion_equivalencia_bac.pas' {FrmImpresionEqBac},
  ComisionesAlMinisterio in '.\ComisionesAlMinisterio.pas' {FrmComAlMinisterio},
  Tutores in '.\Formulario de tutores\Tutores.pas' {FrmTutores},
  CargadePermisosMasivo in '.\Formulario Carga de permisos masivo\CargadePermisosMasivo.pas' {FrmCargaPermisos},
  CambioPassword in '.\Formulario cambio de contraseña\CambioPassword.pas' {FrmCambioPass},
  CargaInasistenciasComision in '.\Formulario carga de inasistencias\CargaInasistenciasComision.pas' {FrmCargaInaComi},
  MensajeError in 'MensajeError.pas' {Frmmensaje},
  Impresiones in 'Impresiones.pas',
  CargadeTrimestres in '.\Formulario Carga de trimestres\CargadeTrimestres.pas' {FrmCargaTri},
  ConsultaReincorporaciones in '.\Formulario Impresiones Reincorporacion\ConsultaReincorporaciones.pas' {FrmConsulRein},
  ActasDisciplinarias in '.\Formulario de Actas disciplinarias\ActasDisciplinarias.pas' {FrmActasDisciplinarias},
  RegularizacionDeMaterias_nuevo in '.\Formulario Regularizacion de Materias_nuevo\RegularizacionDeMaterias_nuevo.pas' {regularizacionMaterias_nuevo},
  Vcl.Themes,
  Vcl.Styles,
  constanciaalumnos2 in '.\Formulario constancia alumno2\constanciaalumnos2.pas' {ConstanciadelAlumno2},
  RegularizacionDeMateriasXComision_nuevo in '.\Formulario Regularizacion de Materias por comision_nuevo\RegularizacionDeMateriasXComision_nuevo.pas' {regularizacionMateriasXComision_nuevo},
  seciones in 'seciones.pas',
  PermisosPorUsuario in '.\Permisos por usuario\PermisosPorUsuario.pas' {FrmPermisosUsuario},
  modulovariable_grf in '.\Modulo Variable\modulovariable_grf.pas' {FrmModVar_grf},
  TipodeExamen in '.\Formulario Listado de actas de Mesas de Examenes\TipodeExamen.pas' {FrmTipoExamen},
  ModificacionInasistenciasComision in '.\Formulario modificacion de inasistencias\ModificacionInasistenciasComision.pas' {FrmModiInaComi},
  gtGmSuiteIntf in 'gtGmSuiteIntf.pas',
  CargaInasistenciasComisionNuevo in '.\Formulario carga de inasistencias nuevo\CargaInasistenciasComisionNuevo.pas' {FrmCargaInaComiNuevo},
  UTHRead in 'UTHRead.pas',
  FuncionesConfiguracion in '.\Esba.funciones\FuncionesConfiguracion.pas',
  FuncionesDB in '.\Esba.funciones\FuncionesDB.pas',
  FuncionesExcel in '.\Esba.funciones\FuncionesExcel.pas',
  FuncionesSystem in '.\Esba.funciones\FuncionesSystem.pas',
  FuncionesText in '.\Esba.funciones\FuncionesText.pas',
  FuncionesVariant in '.\Esba.funciones\FuncionesVariant.pas',
  FuncionesPrint in '.\Esba.funciones\FuncionesPrint.pas',
  Carreras in 'Carreras.pas',
  CargadePermisosWeb in '.\Web\CargadePermisosWeb.pas' {FrmCargaPermisosWeb},
  InscripcionesWeb in '.\Web\InscripcionesWeb.pas' {FrmInscripcionesWeb},
  TablaConfiguraciones in '.\TablaConfiguraciones.pas' {FrmTablaConfiguracionon};

{$R *.RES}

  //***CODIGO DEL PROYECTO**//
Var
  Buffer : array [0..255] of Char;
  IniFile :TiniFile;
  CodUsuario: integer;
begin
 NullStrictConvert := False;
 Application.Initialize;
 GetModuleFileName(HInstance, Buffer, SizeOf(Buffer));
 IniFile := TIniFile.Create(ExtractFilePath(StrPas(Buffer))+'Esba_cnf.ini');
 FrmEsba.UnidadPrograma := IniFile.ReadString('configuracion','Path',''); //ExtractFilePath(StrPas(Buffer));
 FrmEsba.PathListados := IniFile.ReadString('configuracion','PathLst','');
 FuncionesConfiguracion.FPathListados:= FrmEsba.PathListados;
 if FrmEsba.UnidadPrograma='' then begin
   FrmEsba.UnidadPrograma := ExtractFilePath(StrPas(Buffer));
 end;
 FuncionesConfiguracion.Path:=FrmEsba.UnidadPrograma;
 if fileexists(FrmEsba.UnidadPrograma+IniFile.ReadString('configuracion','Skin','xx')) then
      TStyleManager.SetStyle(TStyleManager.LoadFromFile(FrmEsba.UnidadPrograma+IniFile.ReadString('configuracion','Skin','xx')));

 Application.CreateForm(TCustomerData, CustomerData);
  DataModule.CustomerData.FBase.DbName := IniFile.ReadString('configuracion','Base','');
 DataModule.CustomerData.FBase.ConnectParams.UserName:=IniFile.ReadString('configuracion','Usuario','SYSDBA');
 DataModule.CustomerData.FBase.ConnectParams.PassWord:=IniFile.ReadString('configuracion','Pass','masterkey');
 DataModule.CustomerData.FBase.ConnectParams.CharSet:=IniFile.ReadString('configuracion','CharSet','none');
 Application.CreateForm(TFormEsba, FormEsba);
 FormEsba.Caption := FormEsba.Caption+' '+FuncionesSystem.GetAppVersion;
 FormEsba.MnuManager.IniFileName := FrmEsba.UnidadPrograma+CARPETA_NOTAS+'\'+NombreUsuario+'.bar';
 FormEsba.MnuManager.LoadFromIniFile(FrmEsba.UnidadPrograma+CARPETA_NOTAS+'\'+NombreUsuario+'.bar');
 If (Fileexists(FrmEsba.UnidadPrograma+CARPETA_NOTAS+'\'+NombreUsuario+'.not')) Then
       FormEsba.NotasUsuario.Lines.LoadFromFile(FrmEsba.UnidadPrograma+CARPETA_NOTAS+'\'+NombreUsuario+'.not');
 IniFile.Free;
 Application.Run;
End.

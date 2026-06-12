# Documentación de Estructura — Sistema ESBA (Legacy Delphi XE2 + Firebird)

> Generado mediante análisis estático del código fuente en `./Esba.Delphi XE2/ESBA`.
> Sistema de gestión académica del **Instituto de Estudios Superiores de Buenos Aires (A-781)** — alumnos, materias, cursadas, exámenes, certificados y administración de usuarios.

---

## 1. Arquitectura Actual

| Aspecto | Detalle |
|---|---|
| **IDE / Versión** | Delphi XE2 (RAD Studio 9.0 — confirmado por `VCL_Custom_Styles` apuntando a `RAD Studio\9.0\Styles` y por la carpeta del proyecto). `ProjectVersion=13.4` en el `.dproj`. |
| **Tipo de aplicación** | `AppType=Application`, `FrameworkType=VCL` — aplicación de escritorio **VCL Forms (Win32)**. `TargetedPlatforms=1` → solo **Win32** (no Win64). |
| **Proyecto principal** | [ESBA.dpr](./Esba.Delphi%20XE2/ESBA/ESBA.dpr) / `ESBA.dproj` — registra ~60 unidades (`uses`) en el `.dpr`, una por cada formulario. |
| **Acceso a datos** | **FIBPlus** (`pFIBDatabase`, `pFIBQuery`, `pFIBDataSet`, `FIBDatabase`, `FIBQuery`, `FIBDataSet`) — wrapper comercial sobre el cliente nativo de InterBase/Firebird (`gds32.dll` / `fbclient.dll`). **No usa FireDAC ni dbExpress** para el acceso de datos real (esos paquetes figuran en `DCC_UsePackage` pero no se usan en el código). |
| **IBX (InterBase Express)** | Usado de forma puntual solo en [uMensajesError.pas](./Esba.Delphi%20XE2/ESBA/uMensajesError.pas) (unidad `IB`) para detectar y traducir errores de Firebird (violaciones de constraints) a mensajes amigables vía un diccionario `.ini`. |
| **Dataset en memoria** | **kbmMemTable** (`TKbmMemtable`) — se usa extensamente como caché local de datos (ej. grilla principal de alumnos `MtAlumnos` en `FrmEsba`, exportaciones a Excel, listados genéricos). |
| **Conexión a la base** | Configurada en runtime desde `Esba_cnf.ini` (sección `[configuracion]`): `Base=<host>:<ruta>\esba.gdb`, `Usuario=sysdba`, `Pass=masterkey`, `CharSet`. Es decir, **Firebird/InterBase clásico vía TCP**, archivo de base `.gdb` (formato heredado de InterBase / Firebird 1.x-2.x). |
| **DataModule central** | [DataModule.pas](./Esba.Delphi%20XE2/ESBA/DataModule.pas) (`TCustomerData`) — única conexión (`FBase: TpFIBDatabase`) y ~15 pares Transacción/Query/DataSet compartidos por toda la app (patrón "god datamodule"). |
| **UI / Menú principal** | **DevExpress ExpressBars** (`TdxBarManager`, `TdxBarButton`, `TdxBarSubItem`, `cxClasses`, `cxGraphics`, `cxControls`, `dxNavBarCollns`) — el formulario principal `FrmEsba` es una barra de menús/botones dxBar totalmente dinámica, persistida en `.bar` por usuario. |
| **Look & Feel** | **VCL Styles** (skins `.vsf`, ej. `Esbaskn.vsf`, "Iceberg Clásico") + utilidades de terceros en `vclstyle/` (basadas en el proyecto open-source `Vcl.Styles.Utils` de RRUZ) para fixes de DBGrid, DateTimePicker, ColorTabs, etc. sobre VCL Styles. |
| **Controles adicionales** | **RxLib** (`RXToolEdit`, `RXCurrEdit`, `RXCtrls`, `RXDBCtrl`, `RxRichEd`) para editores de fecha/moneda y RichEdit. |
| **Reportes / Impresión** | **Gnostice eDocEngine** (`GmPreview`, `GmTypes`, `gtCstDocEng`, `gtXportIntf`, `GMClasses` — ver [gtGmSuiteIntf.pas](./Esba.Delphi%20XE2/ESBA/gtGmSuiteIntf.pas)) para vista previa/impresión de documentos, combinado con dibujo manual GDI (`Impresiones.pas`, `FuncionesPrint.pas`). |
| **Exportación a Excel** | Automatización OLE (`ExcelXP`) vía [FuncionesExcel.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesExcel.pas). |
| **Gráficos** | `VCLTee` (TeeChart) — usado en `Modulo Variable/modulovariable_grf.pas` para reportes con gráfico + tabla cruzada (`TeeDBCrossTab`). |
| **Correo electrónico** | **Indy** (`IdSMTP`, `IdMessage`, `IdSSLIOHandlerSocketOpenSSL`) para envío de mails por comisión ([enviocorreo.pas](./Esba.Delphi%20XE2/ESBA/Formulario%20de%20envio%20de%20correo%20por%20comision/enviocorreo.pas)), configurado desde `CnfMail.pas`. |
| **Concurrencia** | Una clase `TThread` propia (`TFISQLThread` en [UTHRead.pas](./Esba.Delphi%20XE2/ESBA/UTHRead.pas)) usada por `FrmEsba` para cargar el listado de alumnos en background sin bloquear la UI. |
| **Configuración** | Archivos `.ini` externos al `.exe`: `Esba_cnf.ini` (conexión a BD, paths, skin), `Esba_prg.ini` (preferencias de UI por usuario), `<usuario>.bar` / `<usuario>.not` (layout de menú y notas por usuario). |
| **Empaquetado** | Incluye `esba-upx.exe` (UPX) en la carpeta del proyecto, sugiriendo que el ejecutable final se compacta con UPX antes de distribuirse. |

---

## 2. Mapeo de Archivos Críticos

### 2.1 Núcleo de la aplicación

| Archivo | Tipo | Función principal |
|---|---|---|
| [ESBA.dpr](./Esba.Delphi%20XE2/ESBA/ESBA.dpr) | Proyecto | Punto de entrada. Lee `Esba_cnf.ini`, configura la conexión Firebird (`CustomerData.FBase`), aplica el skin VCL, crea `TCustomerData` y `TFormEsba`, carga el layout de menú (`.bar`) y notas (`.not`) del usuario. |
| [FrmEsba.pas](./Esba.Delphi%20XE2/ESBA/FrmEsba.pas) / `.dfm` | Formulario principal (`TFormEsba`) | **Pantalla central**: menú/barra dxBar con todas las opciones del sistema (organizadas por carrera vía `Carreras.pas`), grilla principal de alumnos (`MtAlumnos`, kbmMemTable cargada en background con `TFISQLThread`), búsqueda global de alumnos, ficha rápida (foto, observaciones, mensajes), acceso a alta/modificación de alumno, y disparo de **todos** los formularios secundarios (cada botón `dxBarButton*Click` abre un formulario hijo). Persiste configuración visual (fuentes/colores de grilla y memo) por usuario. |
| [DataModule.pas](./Esba.Delphi%20XE2/ESBA/DataModule.pas) / `.dfm` | DataModule (`TCustomerData`) | **Único punto de conexión a Firebird** (`FBase: TpFIBDatabase`). Define ~15 transacciones (`FtrBase`, `TrAlumnos`, `TrMaterias`, `TrCursada`, `TrProfesores`, `TRUpdateProf`, `TrSertCerv`, etc.) y sus datasets/queries asociados (`DSAlumnos`, `IbAnalitico`, `IBProfesores`, `IbCursada`, `IbMaterias`, `IbSertCerv`, `IbInscmaterias`, `IbConstancia`, `IbQrVarios`, `IbDsVarios`, etc.), usados de forma transversal por casi todos los formularios. Maneja conexión/desconexión y el manejador global de excepciones (`Application.OnException`). |
| [Carreras.pas](./Esba.Delphi%20XE2/ESBA/Carreras.pas) | Unidad de lógica | Define `TBarraCarreElement` y el array global `BarraCarrerasItems` / `BarraCarrerasGroup` — modela las carreras/ofertas académicas que se listan dinámicamente como grupos del menú (`dxNavBar`) en `FrmEsba`. |
| [seciones.pas](./Esba.Delphi%20XE2/ESBA/seciones.pas) | Unidad de lógica | Control de **sesión única por usuario**: genera un UID (`SetCodSecion`, vía `FuncionesSystem.GenId`) que se guarda en `USUARIOS.UID` y se valida en cada inicio de transacción (`CheckCodSecion`) para detectar logins concurrentes del mismo usuario. |
| [Impresiones.pas](./Esba.Delphi%20XE2/ESBA/Impresiones.pas) | Unidad de lógica | Clase central `TClassImpresiones`: agrupa transacciones/datasets FIBPlus + objetos `TMetaFile`/`TPicture` para componer reportes dibujados a mano y enviarlos a `TGmPreview` (Gnostice) o exportarlos. Es la base de la mayoría de los listados/reportes. |
| [uMensajesError.pas](./Esba.Delphi%20XE2/ESBA/uMensajesError.pas) | Unidad de lógica | Traduce excepciones de Firebird/IB (violaciones de FK/constraint) a mensajes legibles, usando un diccionario cargado desde `.ini` (`CargarDiccionario`, sección `Constraints`). Invocada desde `TCustomerData.Excepciones`. |
| [UTHRead.pas](./Esba.Delphi%20XE2/ESBA/UTHRead.pas) | Unidad de lógica | `TFISQLThread` (descendiente de `TThread`): ejecuta una consulta SQL en un hilo aparte con su propia conexión Firebird y vuelca el resultado a un `TKbmMemtable`, reportando progreso (`TProgressBar`). Usado por `FrmEsba` para no congelar la UI al cargar el padrón de alumnos. |
| [gtGmSuiteIntf.pas](./Esba.Delphi%20XE2/ESBA/gtGmSuiteIntf.pas) | Unidad de lógica | `TgtGmSuiteInterface` — adaptador entre el motor Gnostice eDocEngine (`GmPreview`) y la interfaz de exportación de documentos (`TgtExportInterface`), usado por las pantallas de vista previa/impresión. |
| [MensajeError.pas](./Esba.Delphi%20XE2/ESBA/MensajeError.pas) / `.dfm` | Formulario (`TFrmmensaje`) | Diálogo genérico de mensajes ("Mensaje"), reemplazo custom de `MessageDlg` usado en varios formularios. |
| `MessajeError.pas` | ⚠️ Archivo obsoleto | No está referenciado en el `.dpr`. Parece un duplicado/typo de `MensajeError.pas` — candidato a eliminar en una migración. |
| `vclstyle/*.pas` | Unidades de terceros | Conjunto de unidades (`Vcl.Styles.Utils`, `Vcl.Styles.DbGrid`, `Vcl.Styles.DateTimePickers`, `Vcl.Styles.Fixes`, `Vcl.Styles.ColorTabs`, `Vcl.Styles.FormStyleHooks`, `Vcl.PlatformVclStylesActnCtrls`, etc.) que parchean controles VCL estándar (DBGrid, DateTimePicker, etc.) para que respeten el skin VCL Style activo. |

### 2.2 Unidades de Lógica Compartida (`Esba.funciones/`)

| Archivo | Función principal |
|---|---|
| [FuncionesConfiguracion.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesConfiguracion.pas) | Variables globales de sesión/configuración: `CodUsu` (usuario logueado), `Superv` (flag de supervisor/admin), `Rector`, `Secretaria`, `Path`, `FPathListados`, formato numérico (`FS.DecimalSeparator`). Constantes de carpetas (`CARPETA_PLANTILLA`, `CARPETA_FIRMAS`, `CARPETA_NOTAS`, `CARPETA_CORREO`, `CARPETA_LISTADOS`). |
| [FuncionesDB.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesDB.pas) | **Capa de acceso a datos genérica** sobre FIBPlus: `ExecScript` (ejecuta INSERT/UPDATE/DELETE y devuelve filas afectadas), `ExecScriptMsg` (igual, pero interpreta columnas `ERRCOD`/`ERRMSG` devueltas por procedimientos para mostrar confirmaciones/errores), `ExecQuery_Params` (ejecuta SQL parametrizado, opcionalmente dentro de una transacción existente), `Consulta`/`ConsultaArray` (SELECT de una fila/array a `Variant`), `Carga_MemTable` (vuelca un SELECT a un `TKbmMemtable`), `combobusqueda` (abre el diálogo `Busqueda` y devuelve la fila elegida), `SqlValidate`. |
| [FuncionesExcel.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesExcel.pas) | Exporta `TpFIBDataSet`/`TKbmMemtable` a Excel vía automatización OLE (`Exportar_Excel_DS`, `Exportar_Excel_DS_Hojas` — multi-hoja por campo de corte, `Exportar_Excel_MT`). |
| [FuncionesPrint.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesPrint.pas) | Primitivas de dibujo para reportes sobre `TGmPreview` (Gnostice), ej. `Caja` (dibuja rectángulos/cuadros en el documento). |
| [FuncionesSystem.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesSystem.pas) | Utilidades de sistema: `GetAppVersion`, `GetUserName` (usuario de Windows), `GetVolumeID`, `GetTempDirectory`, `GenID` (generación de identificadores), `CustomMessageDialog`. |
| [FuncionesText.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesText.pas) | Utilidades de formato de texto/dominio: `PasoAMayusculas`, `LPad`, `Corto`, `EditoNumero`, `EncriptoCadena2` (cifrado simple usado para contraseñas), `Cuatrimestre`/`CuatriaLetras`/`LetrasCuat` (conversión de cuatrimestre a texto), `MesALetras`, `Division`, `PoneBarras` (código de barras de libro matriz), `CampoenLista`. |
| [FuncionesVariant.pas](./Esba.Delphi%20XE2/ESBA/Esba.funciones/FuncionesVariant.pas) | Helpers sobre `Variant`: `IfThenVariant`, `TextToNull`. |
| `FuncionesCuotas.pas` | ⚠️ `PrimerVencimCuota` (cálculo de vencimiento de cuotas). **No está referenciado por ningún otro archivo ni en el `.dpr`** — parece código muerto/no integrado. |

### 2.3 Módulos Reutilizables (`Modulo Variable/`)

| Archivo | Clase | Función principal |
|---|---|---|
| [modulovariable.pas](./Esba.Delphi%20XE2/ESBA/Modulo%20Variable/modulovariable.pas) | `TFrmModVariable` ("Principal") | **Formulario de listado genérico**: grilla (`DBGrid`) + dataset FIBPlus parametrizable + botones de exportar a Excel / imprimir / salir. Es la base reutilizada por la mayoría de las pantallas "Listado de...". |
| [modulovariable_grf.pas](./Esba.Delphi%20XE2/ESBA/Modulo%20Variable/modulovariable_grf.pas) | `TFrmModVar_grf` | Variante de `modulovariable` con **gráfico** (`TDBChart` + `TDBCrossTabSource`, TeeChart) además de la grilla, para reportes estadísticos. |
| [parametros.pas](./Esba.Delphi%20XE2/ESBA/Modulo%20Variable/parametros.pas) | `TFrmParametros` ("Parametros") | **Formulario dinámico de parámetros**: a partir de un array de registros `Param` (etiqueta, control, tipo, SQL de combo, etc.) genera en runtime los controles de filtro para alimentar a `modulovariable`/`modulovariable_grf` y a los reportes de `Impresiones.pas`. |
| [Busqueda.pas](./Esba.Delphi%20XE2/ESBA/Modulo%20Variable/Busqueda.pas) | `TFrmBusqueda` ("Busqueda") | Diálogo modal genérico de **búsqueda en grilla** (usado por `FuncionesDB.combobusqueda` y por formularios de alta para buscar registros relacionados, p.ej. alumnos, materias). |

### 2.4 Formularios de UI por Área Funcional

#### Autenticación / Sesión
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario de Inicio de Sesion/sesion.pas` | `Tiniciodesesion` | "Inicio de Sesión" | Login: valida usuario/clave contra tabla `USUARIOS` (`CustomerData.FDSUsuarios`), comparando contraseña cifrada con `FuncionesText.EncriptoCadena2`. |
| `Formulario cambio de contraseña/CambioPassword.pas` | `TFrmCambioPass` | "Cambio de contraseña" | Cambio de clave del usuario logueado. |

#### Gestión de Alumnos
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario Alta de alumno/FrmAltaAlumno.pas` | `TAltaAlumno` | "AltaAlumno" | Formulario maestro de **alta y modificación de datos personales del alumno** (datos civiles, domicilio, estudios previos, foto, estado, opciones web). Reutilizado en modo alta y edición desde `FrmEsba`. |
| `Formulario de Cambio de Cod_Alu y LM/CambioCodAlu_LM.pas` | `TModifCodAlu_LM` | "ModifCodAlu_LM" | Cambia el código de alumno (DNI/tipo doc) y/o el número de libro matriz, delegando la validación/actualización al **SP `XXX_CAMBIA_DNI_LM`**. |
| `Formulario de tutores/Tutores.pas` | `TFrmTutores` | "Tutores del Alumno" | ABM de tutores/responsables asociados a un alumno. |
| `Formulario de Actas disciplinarias/ActasDisciplinarias.pas` | `TFrmActasDisciplinarias` | "Actas Disciplinarias del Alumno" | Registro de actas/sanciones disciplinarias por alumno. |
| `Eliminacion de Profesores/ElimProfes.pas` | `TEliminacionProfes` | "Eliminación de Profesores" | (pese al nombre de carpeta, gestiona baja de profesores — ver sección Administración). |

#### Gestión Académica (Materias / Comisiones / Cursada)
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario Alta de materias/altamodifmaterias.pas` | `TAlMoMaterias` | "Alta - Modificación de Materias" | ABM del catálogo de materias (`MATERIAS`). |
| `formulario carga de comisiones/cargacomisiones.pas` | `TFrmComArmadas` | "Comisiones armadas" | Armado/gestión de comisiones (grupos de cursada) por materia/cuatrimestre. |
| `Formulario Inscripcion de Materias (Sirve)/InscripcionDeMaterias.pas` | `TInscripcionMaterias` | "Inscripción de Materias" | Inscribe a un alumno en materias/comisiones de su carrera (`CURSADA`). |
| `Formulario Regularizacion de Materias_nuevo/RegularizacionDeMaterias_nuevo.pas` | `TregularizacionMaterias_nuevo` | "Regularización..." | Marca/gestiona la regularización de materias de un alumno. |
| `Formulario Regularizacion de Materias por comision_nuevo/RegularizacionDeMateriasXComision_nuevo.pas` | `TregularizacionMateriasXComision_nuevo` | "Regularización..." | Igual que el anterior pero en **lote, por comisión completa**. |
| `Formulario alta-modificacion de analitico/ModifAnalitico.pas` | `TAMAnalitico` | "Alta, modificación de analítico" | ABM del histórico académico/analítico del alumno (`ANALITIC`), incluye cálculo de promedio (ver SP `XXX_PROMEDIO_GRAL`). |
| `Formulario Carga de trimestres/CargadeTrimestres.pas` | `TFrmCargaTri` | "Carga de Cuatrimestres" | Configuración de cuatrimestres/trimestres (`TBL_CUAT`, `TBL_TRIM`) y feriados (`TBL_FERIADOS`). |

#### Asistencias y Permisos
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario carga de inasistencias/CargaInasistenciasComision.pas` | `TFrmCargaInaComi` | "Carga de Inasistencias" | Carga de inasistencias por comisión/fecha (versión original). |
| `Formulario carga de inasistencias nuevo/CargaInasistenciasComisionNuevo.pas` | `TFrmCargaInaComiNuevo` | "Carga de Inasistencias" | Versión rediseñada del formulario anterior (contiene además una copia de respaldo en subcarpeta `bkp/`). |
| `Formulario modificacion de inasistencias/ModificacionInasistenciasComision.pas` | `TFrmModiInaComi` | "Carga de Inasistencias" | Edición/corrección de inasistencias ya cargadas. |
| `Formulario Listado de Asistencias/lstplanasis.pas` | `TFrmlstplanasis` | "Listado de Planillas de Asistencia" | Reporte de planillas de asistencia (usa `XXX_FALTAS_IMPRESI`, `XXX_FALTAS_PASLIBRE`). |
| `Formulario Carga de permisos masivo/CargadePermisosMasivo.pas` | `TFrmCargaPermisos` | "Carga de permisos de examenes" | Carga masiva de permisos de examen para varios alumnos. |
| `Formulario Permisos de Examenes/PermisoExamen.pas` | `TPermisosExamen` | "Permisos de Exámenes" | ABM de permisos de examen individuales (`PERMEXA`). |
| `Web/CargadePermisosWeb.pas` | `TFrmCargaPermisosWeb` | "Permisos cargados via Web" | Sincroniza/revisa permisos de examen ingresados desde el portal web (misma base Firebird). |

#### Exámenes y Mesas
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario Alta-Modificacion-Baja Mesas de Examen/MesasExamen.pas` | `Tmesasexamenes` | "Mesas de Exámenes" | ABM de mesas de examen (fecha, tribunal, materia). |
| `Formulario Listado de actas de Mesas de Examenes/lstactasMesas.pas` | `TFrmlstMesas` | "Actas Volantes" | Listado/impresión de actas de mesas de examen. |
| `Formulario Listado de actas de Mesas de Examenes/TipodeExamen.pas` | `TFrmTipoExamen` | "Tipo de Examen" | Selector auxiliar de tipo de examen para el listado anterior. |
| `Formulario Notas de examenes finales/NotasExamenFinal.pas` | `TNotasExamenes` | "Notas de Exámenes Finales" | Carga de notas de exámenes finales por alumno/mesa (usa `XXX_MATERIAS_FINALES`). |
| `Formulario finales por mesa y comison/FinalesxMesayComision.pas` | `TFinalesmesacom` | "Finales por mesa y comisión" | Listado/carga de finales agrupados por mesa y comisión. |
| `Formulario Listado de actas de examenes/lstactasexamenes.pas` | `TFrmlstactas` | "Actas de Exámenes" | Impresión de actas de exámenes finales. |
| `Formulario Listado de actas de A-REGULAR/lstactasARegular.pas` | `TFrmlstARegular` | "Actas de A/Regular" | Impresión de actas para alumnos libres/regulares. |
| `Formulario listado notas y practico/lstNotasyPractico.pas` | `TFrmPractNotas` | "FrmPractNotas" | Listado combinado de notas y trabajos prácticos. |

#### Constancias, Certificados y Equivalencias
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario  constancia alumno (Sirve)/constanciaalumnos.pas` | `TConstanciadelAlumno` | "Constancia del Alumno" | Genera constancias de alumno (en trámite, certificado de estudios) vía `XXX_IMPRIME_CTT`/`XXX_IMPRIME_PASE`/`XXX_PARRAFO_CONSTANCIA`. Incluye un archivo de respaldo `constanciaalumnos BKP 26092012.pas` (⚠️ no usado por el `.dpr`). |
| `Formulario constancia alumno2/constanciaalumnos2.pas` | `TConstanciadelAlumno2` | "Constancia del Alumno" | Versión más nueva/alternativa de la constancia de alumno (la que invoca `FrmEsba` actualmente). |
| `Formulario Constancia Alumno Regular/constanciaalumnoregular.pas` | `TConstAlumnoRegular` | "Constancia de Alumno Regular" | Constancia de alumno regular (certificado de regularidad), con envío por mail al alumno. |
| `Formulario Certificacion de Servisios/CertServ.pas` | `TFrmCertServ` | "Certificación de servicios" | Certificación de servicios docentes (`CERTSERV`). |
| `Formulario Equivalencias/Equivalencia.pas` | `TFRMEquivalencias` | "Alta de equivalencias" | ABM de equivalencias de materias (para alumnos provenientes de otras instituciones/carreras). |
| `Formulario impresion equivalencia bachiller/lst_impresion_equivalencia_bac.pas` | `TFrmImpresionEqBac` | "Impresión Equivalencia Bachiller" | Impresión de equivalencias para título de bachiller. |
| `Formulario impresion equivalencia terciaria/lst_impresion_equivalencia_terc.pas` | `TFrmImpresionEqTerc` | "Impresión Equivalencia de Terciaria" | Impresión de equivalencias para títulos terciarios (`XXX_CONSTANCIA_TERCIARIA`). |
| `Formulario Listado de actas de reincorporacion/lstactasreincorporacion.pas` | `TFrmlstReincorporacion` | "Actas de Reincorporaciones" | Impresión de actas de reincorporación de alumnos. |
| `Formulario Impresiones Reincorporacion/ConsultaReincorporaciones.pas` | `TFrmConsulRein` | "Impresiones de Reincorporaciones" | Consulta/listado previo a la impresión de reincorporaciones. |

#### Administración del Sistema (Usuarios, Profesores, Configuración)
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario Alta de Usuarios/AltaUsuario.pas` | `TFrmAltaUsuario` | "Alta de usuarios" | ABM de usuarios del sistema (`USUARIOS`). |
| `Formulario baja de usuarios/BajaUsuarios.pas` | `TFrmBajaUsuarios` | "Baja de Usuarios" | Baja/inactivación de usuarios. |
| `Permisos por usuario/PermisosPorUsuario.pas` | `TFrmPermisosUsuario` | "Permisos por Usuario" | Asigna permisos por carrera/módulo a cada usuario (tabla `BARRA_SEGU`, referenciada también desde `FrmEsba`). |
| `Alta - Modificacion de profesores Hecho por 2º vez/FrmAltaModProfes.pas` | `TFrmAltaModifProf` | "Alta, modificación de profesores" | ABM de profesores/docentes (`DOCENTES`). El nombre de carpeta indica que es una **reescritura** de un formulario anterior. |
| `Eliminacion de Profesores/ElimProfes.pas` | `TEliminacionProfes` | "Eliminación de Profesores" | Baja de profesores. |
| `ComisionesAlMinisterio.pas` | `TFrmComAlMinisterio` | "Comisiones al Ministerio" | Generación de información de comisiones para reportar al Ministerio de Educación. |
| `TablaConfiguraciones.pas` | `TFrmTablaConfiguracion` | "Configuraciones" | Editor de parámetros/configuraciones generales del sistema almacenados en BD. |
| `Formulario de configuracion de correo/CnfMail.pas` | `TFrmCnfMail` | "Configuración de Correo" | Configura el servidor SMTP (Indy) usado para el envío de correos. |
| `Formulario de envio de correo por comision/enviocorreo.pas` | `TFrmenviomail` | "Envío de correo electrónico por comisión" | Envío masivo de mails a alumnos de una comisión (vía `IdSMTP` + `IdSSLIOHandlerSocketOpenSSL`). |

#### Vista Previa / Impresión
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Formulario Impresion/Imprimir.pas` | `TFormPreview` | "Vista Previa" | Contenedor de vista previa de impresión basado en `TGmPreview` (Gnostice), usado por todos los reportes generados por `Impresiones.pas`. |

#### Integración Web
| Carpeta / Archivo | Clase | Caption | Función |
|---|---|---|---|
| `Web/InscripcionesWeb.pas` | `TFrmInscripcionesWeb` | "Inscripciones Web" | Revisión/procesamiento de inscripciones a materias generadas desde el portal web del alumno (misma base Firebird, probablemente tablas/área separada). |
| `Web/CargadePermisosWeb.pas` | `TFrmCargaPermisosWeb` | "Permisos cargados via Web" | Igual que arriba, para permisos de examen solicitados vía web. |

---

## 3. Estado de la Base de Datos Firebird

- **No se encontró ningún script `.sql`, backup `.fbk`/`.gbk` ni archivo `.gdb`/`.fdb` en el repositorio** — el esquema de la base de datos **no está versionado junto al código**. El archivo de datos (`esba.gdb`, formato heredado InterBase/Firebird) reside en el servidor de producción (`192.168.0.102:/home/damian/sistemas/Data/esba.gdb` según `Esba.ini`). Cualquier migración deberá partir de un `gbak` de la base real para conocer el DDL completo (tablas, índices, triggers, generadores).

- **La lógica de negocio está repartida entre el cliente Delphi y la base**:
  - La **mayoría de las operaciones CRUD** (altas, bajas, modificaciones de `ALUMNOS`, `MATERIAS`, `CURSADA`, `USUARIOS`, `DOCENTES`, etc.) se realizan mediante **SQL dinámico construido por concatenación de strings en Delphi** (`FuncionesDB.ExecScript`/`ExecQuery_Params`/`Consulta`), no mediante stored procedures. Esto implica:
    - Riesgo de **SQL injection** si algún valor proviene de input de usuario sin sanitizar (se observan concatenaciones directas de `.Text` de controles en varios formularios, ej. `FrmEsba.ModificarAlumnoClick`, `FrmEsba.AbroBDDAlumnosTHRead`).
    - Las reglas de validación/formato están mayormente en el código Delphi (`FuncionesText`, `FuncionesVariant`, etc.), no en triggers.
  - **Sí existen procedimientos almacenados Firebird seleccionables** (convención de nombres con prefijo `XXX_`), invocados con la sintaxis `SELECT ... FROM XXX_PROCEDIMIENTO(:param1, :param2, ...)`. Procedimientos detectados en el código:

    | Procedimiento | Usado en | Propósito aparente |
    |---|---|---|
    | `XXX_CAMBIA_DNI_LM` | `CambioCodAlu_LM.pas` | Cambia DNI/código de alumno y libro matriz, validando duplicados/consistencia. |
    | `XXX_IMPRIME_CTT` | `constanciaalumnos.pas` | Datos para constancia de "Certificado de Estudios en Trámite". |
    | `XXX_IMPRIME_PASE` | `constanciaalumnos.pas` | Datos para pase/constancia de alumno. |
    | `XXX_PARRAFO_CONSTANCIA` | `constanciaalumnos.pas` | Genera el texto/párrafo dinámico de una constancia. |
    | `XXX_PROMEDIO_GRAL` | `ModifAnalitico.pas` | Calcula el promedio general del analítico del alumno. |
    | `XXX_FALTAS_IMPRESI` / `XXX_FALTAS_PASLIBRE` | `lstplanasis.pas` | Cálculo de inasistencias para impresión / pase a libre. |
    | `XXX_MATERIAS_FINALES` | listados de actas/notas finales | Materias habilitadas para examen final. |
    | `XXX_CONSTANCIA_TERCIARIA` | `lst_impresion_equivalencia_terc.pas` | Datos para constancia/equivalencia terciaria. |
    | `XXX_OBSERV_PANTA` | `FrmEsba.pas` (grilla principal) | Observaciones/mensajes/color a mostrar por alumno en la pantalla principal. |
    | `XXX_CONF` | varios | Tabla/función de configuración general. |

  - **Patrón de manejo de errores `ERRCOD`/`ERRMSG` (o `FERRCOD`/`FERRMSG`)**: varios de estos procedimientos devuelven columnas de código y mensaje de error que `FuncionesDB.ExecScriptMsg` interpreta así:
    - `2` → error bloqueante (`MessageDlg` tipo error, rollback).
    - `1` → confirmación al usuario (sí/no) antes de commitear.
    - `0` con mensaje → aviso informativo, pero continúa y commitea.
    - Sin código → ejecución normal.
    Esto sugiere que **parte de las validaciones de negocio críticas sí viven en la base** (dentro de estos SP), aunque el flujo CRUD general no.

- **Tablas referenciadas directamente desde el código** (lista no exhaustiva, recolectada de las consultas embebidas): `ALUMNOS`, `CARRERA`, `MATERIAS`, `CURSADA`, `LOG_CURSADA`, `ANALITIC`, `DOCENTES`, `USUARIOS`, `PERMEXA`, `CERTSERV`, `TBL_CUAT`, `TBL_TRIM`, `TBL_FERIADOS`, `BARRA_SEGU`.

- **No se encontraron referencias a `GENERATOR`/`GEN_ID()`** en el código fuente — la generación de identificadores parece resolverse vía `FuncionesSystem.GenID` (lado cliente) o dentro de los SP `XXX_*`; debe confirmarse contra el DDL real de la base.

---

## 4. Dependencias de Terceros

| Componente / Librería | Uso en el proyecto | Notas |
|---|---|---|
| **FIBPlus** (`pFIBDatabase`, `pFIBQuery`, `pFIBDataSet`, `FIBDatabase`, `FIBQuery`, `FIBDataSet`) | Capa de acceso a datos completa (conexión, transacciones, queries, datasets). | Componente comercial (Pegas/devrace). Es la dependencia **más crítica** para cualquier migración — toda la app depende de su API. |
| **IBX** (unidad `IB`) | Solo para tipos de excepción de Firebird en `uMensajesError.pas`. | Viene con Delphi; uso mínimo y aislado. |
| **kbmMemTable** (`TKbmMemtable`) | Datasets en memoria para grillas, caché y exportaciones (grilla principal de alumnos, `modulovariable`, `Carga_MemTable`, `Exportar_Excel_MT`). | Componente comercial (Components4Developers). |
| **DevExpress ExpressBars / cxLibrary** (`TdxBarManager`, `TdxBarButton`, `TdxBarSubItem`, `TdxBarPopupMenu`, `cxClasses`, `cxGraphics`, `cxControls`, `dxNavBarCollns`, `dxSkinsCore`, `dxSkinsdxBarPainter`) | Menú/barra de herramientas principal y navegación por carreras en `FrmEsba`. | Es la base de toda la navegación de la app — reemplazarlo implica rediseñar el shell de la aplicación. |
| **RxLib** (`RXToolEdit`, `RXCurrEdit`, `RXCtrls`, `RXDBCtrl`, `RxRichEd`) | Editores de fecha (`TDateEdit`), moneda, controles enlazados a datos adicionales, RichEdit. | Librería open-source clásica de Delphi. |
| **Gnostice eDocEngine** (`GmPreview`, `GmTypes`, `gtCstDocEng`, `gtXportIntf`, `GMClasses`, `gmprinter`) | Motor de vista previa/impresión/exportación de documentos para todos los reportes (`Imprimir.pas`, `Impresiones.pas`, `gtGmSuiteIntf.pas`). | Componente comercial — los reportes están dibujados "a mano" (GDI) sobre el canvas de Gnostice, **no** usan FastReport/QuickReport/ReportBuilder. |
| **TeeChart (VCLTee)** (`VCLTee.Chart`, `VCLTee.DBChart`, `VCLTee.Series`, `VCLTee.TeeDBCrossTab`) | Gráficos y tablas cruzadas en `Modulo Variable/modulovariable_grf.pas`. | Incluido con Delphi (edición estándar de TeeChart). |
| **Indy** (`IdSMTP`, `IdMessage`, `IdMessageClient`, `IdSSLIOHandlerSocketOpenSSL`, etc.) | Envío de correo SMTP/SSL (`enviocorreo.pas`, `CnfMail.pas`). | Incluido con Delphi. |
| **Excel OLE Automation** (`ExcelXP`) | Exportación de datasets/memtables a Excel (`FuncionesExcel.pas`). | Requiere Microsoft Excel instalado en el cliente — fuerte acoplamiento al entorno Windows. |
| **VCL Styles + `Vcl.Styles.Utils` (RRUZ, open-source)** | Skins de la aplicación (`.vsf`, ej. `Esbaskn.vsf`, "Iceberg Clásico") y fixes para que DBGrid/DateTimePicker/etc. respeten el estilo (`vclstyle/*.pas`). | Código fuente de terceros incluido directamente en el proyecto. |
| **UPX** (`esba-upx.exe`) | Empaquetador del ejecutable final (presente en la carpeta del proyecto, no es una dependencia de compilación). | Verificar si forma parte del proceso de build/deploy. |

---

## 5. Observaciones para una Futura Migración

- El **DataModule único** (`TCustomerData`) con ~15 transacciones/datasets compartidos globalmente es el principal punto de acoplamiento: casi todos los formularios acceden directamente a `DataModule.CustomerData.<algo>`, lo que dificulta extraer módulos de forma aislada.
- La regla de negocio está **fragmentada entre Delphi y SPs `XXX_*`** — antes de migrar, conviene extraer (via `gbak`/`isql`) el código fuente PSQL de todos los procedimientos `XXX_*`, triggers y generadores de `esba.gdb`, ya que **no existe en este repositorio**.
- Hay **artefactos obsoletos** que conviene excluir del análisis funcional: `MessajeError.pas` (duplicado no usado), `constanciaalumnos BKP 26092012.pas` (backup), `FuncionesCuotas.pas` (código muerto), y la copia en `Formulario carga de inasistencias nuevo/bkp/`.
- El acoplamiento a **componentes comerciales Win32** (FIBPlus, kbmMemTable, DevExpress dxBar, Gnostice eDocEngine, RxLib) y a **automatización OLE de Excel** son las barreras técnicas más relevantes para portar a .NET/multiplataforma — cada uno requiere un equivalente funcional (p. ej. Firebird .NET provider, grids web/desktop, motor de reportes tipo PDF, librería de Excel como ClosedXML/EPPlus).
- El **patrón `ERRCOD`/`ERRMSG`** de los SP `XXX_*` es un buen candidato a modelarse explícitamente como "resultado de operación" (tipo `Result<T>`/respuesta con código+mensaje) en una nueva arquitectura.

object CustomerData: TCustomerData
  OldCreateOrder = True
  OnCreate = DataModuleCreate
  Height = 430
  Width = 640
  object FBase: TpFIBDatabase
    DBParams.Strings = (
      'lc_ctype=UTF-8')
    SQLDialect = 3
    Timeout = 0
    LibraryName = 'fbclient.dll'
    WaitForRestoreConnect = 0
    Left = 16
    Top = 8
  end
  object FtrBase: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 64
    Top = 8
  end
  object FTrUsuarios: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 256
    Top = 8
  end
  object TrQrVarios: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 80
    Top = 72
  end
  object FDsUsuarios: TpFIBDataSet
    SelectSQL.Strings = (
      'SELECT NOMBRE, CODUSU, PASSWD, CAMPASS, SUPERV'
      'FROM USUARIOS '
      'WHERE NOMBRE = :NOM')
    Transaction = FTrUsuarios
    Database = FBase
    Left = 208
    Top = 8
  end
  object IbQrVarios: TpFIBQuery
    Transaction = TrQrVarios
    Database = FBase
    Left = 24
    Top = 72
  end
  object TrInscMaterias: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 256
    Top = 72
  end
  object TrDlVArios: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 88
    Top = 136
  end
  object IBDlVArios: TpFIBQuery
    Transaction = TrDlVArios
    Database = FBase
    Left = 24
    Top = 136
  end
  object TrUpVarios: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 264
    Top = 136
  end
  object IbupVArios: TpFIBQuery
    Transaction = TrUpVarios
    Database = FBase
    Left = 208
    Top = 136
  end
  object Trconstancia: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 400
    Top = 136
  end
  object TrDsVarios: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 408
    Top = 72
  end
  object IbDsVarios: TpFIBDataSet
    Transaction = TrDsVarios
    Database = FBase
    Left = 344
    Top = 72
  end
  object TrValidacion: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 424
    Top = 8
  end
  object IbValidacion: TpFIBQuery
    Transaction = TrValidacion
    Database = FBase
    Left = 352
    Top = 8
  end
  object IbCursada: TpFIBQuery
    Transaction = TrCursada
    Database = FBase
    SQL.Strings = (
      'SELECT C.COD_ALU, C.CUTUCO, C.COD_MAT, C.CUA_ANIO, C.CONDICION,'
      
        '       C.TP_EVA, C.RECUP , C.TP_EVA2, C.RECUP2, C.REGULAR, C.TOT' +
        '_HORAS,'
      
        '       C.INASIST, C.JUSTIF, C.CONDICION, C.FINAL1, C.FECHA1, C.F' +
        'INAL2,'
      '       C.FECHA2, C.MATRIZ, A.COD_ALU, A.APELLIDO, A.NOM_APE'
      'FROM  CURSADA C'
      'LEFT OUTER JOIN ALUMNOS A ON C.COD_ALU = A.COD_ALU'
      
        'WHERE C.CUTUCO = :COMI  AND C.COD_MAT  = :MAT AND C.CUA_ANIO = :' +
        'CUA AND C.CARRE=:CARRE AND'
      '      TRIM(C.CONDICION) <> '#39'REGULAR'#39
      'ORDER BY C.CONDICION,A.APELLIDO,A.NOM_APE')
    Left = 24
    Top = 200
  end
  object TrCursada: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 80
    Top = 200
  end
  object IbMaterias: TpFIBQuery
    Transaction = TrMaterias
    Database = FBase
    SQL.Strings = (
      
        'SELECT CODMATERI, DESCRIPCI, CUATRIM, EQUIVALE FROM MATERIAS WHE' +
        'RE CODCARRE = :CODCARRE')
    Left = 208
    Top = 200
  end
  object TrMaterias: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 256
    Top = 200
  end
  object IbAnalitico: TpFIBDataSet
    SelectSQL.Strings = (
      
        'SELECT A.COD_MAT, M.SIGLA, A.CUA_ANIO,A.NOTA_MAT, A.FEC_FINAL,A.' +
        'MATRIZ,'
      
        '       A.CONDICION,A.INSTITUT,A.CARAC,A.ACTINT,A.ACTDGE,A.COLEGI' +
        'O,'
      '       A."PLAN", A.A_C, A.FACTFIN, A.FEXDESCRI'
      'FROM ANALITIC A'
      
        'LEFT OUTER JOIN MATERIAS M ON A.COD_MAT=M.CODMATERI AND A.CARRE=' +
        'M.CODCARRE'
      'WHERE A.COD_ALU=:CODALU AND A.CARRE=:CARRE')
    Transaction = TrAnalitico
    Database = FBase
    Left = 344
    Top = 200
  end
  object TrAnalitico: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 408
    Top = 200
  end
  object IBUpdateProf: TpFIBQuery
    Transaction = TRUpdateProf
    Database = FBase
    Left = 512
    Top = 8
  end
  object TRUpdateProf: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 552
    Top = 8
  end
  object IBProfesores: TpFIBDataSet
    SelectSQL.Strings = (
      'SELECT  DOCENTE,    FECHA_ING,    FECHA_BAJ,    DI_ECCION,'
      
        '        PISO,    DEPTO,    TELEFONO_P,    TELEFONO_M,    INTERNO' +
        ',      '
      
        '        LOCALIDAD,    TITULO,    UNIVERDID,    NROREGIST,    ANT' +
        'DOCENT,    '
      '        ANTDOCSUP,    ANTCATEDR,    CODPROFES, '
      
        '        NRODOCUM,    TIPODOC,    FEC_NAC,    COD_POST, NACIONALI' +
        ', LICENCIA, '
      '        LICENFECH, FLOOR((CURRENT_DATE-ANTCATEDR)/365) AS ANIOS'
      'FROM DOCENTES'
      'ORDER BY CODPROFES')
    Transaction = TrProfesores
    Left = 512
    Top = 64
  end
  object TrProfesores: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 560
    Top = 64
  end
  object TrSertCerv: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 560
    Top = 112
  end
  object IbSertCerv: TpFIBDataSet
    SelectSQL.Strings = (
      'SELECT C.COD_PROFES, C.COD_CARRE,  M.SIGLA, C.COMISION,'
      '       C.CUA_ANIO, C.HORAS, C.TIT_SUP, C.INDICE, C.COD_MAT'
      'FROM CERTSERV C'
      
        'LEFT OUTER JOIN MATERIAS M ON C.COD_MAT=M.CODMATERI AND C.COD_CA' +
        'RRE=M.CODCARRE'
      'WHERE C.COD_PROFES=:CODPROF')
    Transaction = TrSertCerv
    Database = FBase
    Left = 504
    Top = 112
  end
  object DsAlumnos: TpFIBDataSet
    SelectSQL.Strings = (
      
        'SELECT A.COD_ALU, MATRIZ, APELLIDO, NOM_APE,MAIL, EXT_POR, NACIO' +
        'NAL, '
      
        '       EST_CIV, SEXO, FEC_NAC, LUG_NAC,PCIA_NAC, DOMI, LOCALI, C' +
        'OD_POS, CAR_TEL, TELE, '
      
        '       FEC_ING, CTT, CA, DNI, FECH_CTT,EST_CIV,CPRIM,APRIM,TPRIM' +
        ',CSECU,'
      
        '       ASECU,TSECU,TERCI,ATERCI,TTERCI,EMPRE,RUBRO,CARGO,ANTI,DO' +
        'MI_1,TELE_1,INTER, '
      
        '       BAJA, CELULAR, FFOTO, FAPTFIS, FAPTFEC, FOTO, X.DATOS, A.' +
        'FCLAWEB, A.FUSUWEB, X.FCOLOR, X.FMENSAJE,'
      '      X.OBSERV '
      'FROM ALUMNOS A'
      
        'LEFT OUTER JOIN XXX_OBSERV_PANTA(A.COD_ALU, A.CARRE) X ON A.COD_' +
        'ALU=X.COD_ALU'
      'WHERE CARRE = :CARRERA AND BAJA = :BAJAS'
      'ORDER BY APELLIDO, NOM_APE')
    Transaction = TrAlumnos
    Database = FBase
    Left = 16
    Top = 288
  end
  object TrAlumnos: TpFIBTransaction
    DefaultDatabase = FBase
    TimeoutAction = TARollback
    BeforeStart = BaseBeforeStart
    Left = 56
    Top = 288
  end
  object IbInscmaterias: TpFIBDataSet
    SelectSQL.Strings = (
      'SELECT C.MATRIZ, C.CUTUCO, C.COD_MAT,M.DESCRIPCI,'
      '       C.CONDICION,C.CUA_ANIO'
      'FROM CURSADA C'
      
        'LEFT OUTER JOIN MATERIAS M ON C.COD_MAT=M.CODMATERI AND C.CARRE=' +
        'M.CODCARRE'
      'WHERE (COD_ALU = :CodigoAlumno) AND '
      
        '      CONDICION  IN ('#39'REGULAR'#39','#39'CURSANDO'#39','#39'RECURSANDO'#39','#39'RECURSA'#39 +
        ','#39'P/EQUIVALEN'#39')'
      '      AND C.CARRE=:CARRE'
      'ORDER BY CUTUCO,COD_MAT,CONDICION')
    Transaction = TrInscMaterias
    Database = FBase
    Left = 184
    Top = 72
  end
  object IbConstancia: TpFIBDataSet
    SelectSQL.Strings = (
      
        'SELECT CUATRIM, CODMAT, SIGLA, DESCRIPCI,CUTUCO, CONDICION, CUAA' +
        'NIO, NOTA, FECHA, '
      
        '             INSTITUTO, CARACT, ACTINT, ACTFIN, ACTDEGP, ACTSNE,' +
        ' ANUAL, COLOR, FONTCOLOR,'
      '             VENCIM, EXIMDESC'
      'FROM XXX_CONSTANCIA_TERCIARIA(:CODALU, :CARRE)')
    Transaction = Trconstancia
    Database = FBase
    Left = 344
    Top = 136
  end
end

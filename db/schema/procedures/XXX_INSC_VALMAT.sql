SET TERM ^ ;

ALTER PROCEDURE XXX_INSC_VALMAT (CODALU VARCHAR(11) CHARACTER SET NONE,
CARRE VARCHAR(6) CHARACTER SET NONE,
CODMAT CHAR(2) CHARACTER SET NONE,
TIPO CHAR(1) CHARACTER SET NONE)
RETURNS (FERRCOD INTEGER,
FERRMSG VARCHAR(500) CHARACTER SET NONE,
CONDICION VARCHAR(20) CHARACTER SET NONE,
MATERIA VARCHAR(60) CHARACTER SET NONE,
FERRWEB VARCHAR(500) CHARACTER SET NONE,
MAXCUATRIMAPROB INTEGER,
CUATGRABA BOOLEAN)
AS 
declare variable CORRELATI varchar(100);
declare variable BANDERA char(1);
declare variable CONT integer;
declare variable SIGO char(1);
declare variable CODMATVER char(2);
declare variable MAT_ADEUDA varchar(60);
declare variable CUATRIM integer;
declare variable MAX_CUAT_CHECK integer;
declare variable MATERIAS_ANIO integer;
declare variable MATERIAS_APROB integer;
begin
  --TIPO I=INSCRIPCION DE MATERIAS A=MODIFICACION DE ANALITICO
  SELECT TRIM(M.DESCRIPCI), TRIM(M.CORRELATIV), m.cuatrim
  FROM MATERIAS M
  WHERE M.CODMATERI=:CODMAT AND M.CODCARRE=:CARRE
  INTO :MATERIA, :CORRELATI, :cuatrim;

  FERRCOD=0;
  FERRMSG='';
  CUATGRABA=TRUE;
  --VERIFICO QUE NO EXISTA LA MATERIA
  SELECT C.CONDICION
  FROM CURSADA C
  WHERE C.COD_ALU=:CODALU AND C.CARRE=:CARRE AND C.COD_MAT=:CODMAT
  INTO :CONDICION;

  IF (CONDICION IS NOT NULL) THEN
   BEGIN
      FERRCOD=2;
      FERRMSG=MATERIA||', esta materia cargada y esta en condicion de '||CONDICION;
      FERRWEB='Esta materia ya esta dada de alta y esta en condicion de '||CONDICION;
      CUATGRABA=FALSE;
   END
  IF (EXISTS(SELECT 1 FROM ANALITIC A WHERE  A.COD_ALU=:CODALU AND A.CARRE=:CARRE AND A.COD_MAT=:CODMAT)) THEN
    BEGIN
      SELECT CONDICION FROM ANALITIC A WHERE  A.COD_ALU=:CODALU AND A.CARRE=:CARRE AND A.COD_MAT=:CODMAT INTO :CONDICION;
      FERRCOD=2;
      FERRMSG=MATERIA||', esta aprobada por final/Equivalencia';
      FERRWEB='Esta materia esta aprobada por final/Equivalencia';
      CUATGRABA=FALSE;
    END

   /* no vas mas a pedido de sandra
     if (cuatrim>=3 AND CARRE<>'650') then begin
       if (cuatrim in (3,4)) then
          max_cuat_check=2;
       if (cuatrim in (5,6)) then
          max_cuat_check=4;
       if (cuatrim in (7,8)) then
          max_cuat_check=6;

       select count(*), sum(case when a.condicion is null then 0 else 1 end)
       from materias m
       left outer join analitic a on m.codcarre=a.carre and m.codmateri=a.cod_mat and A.COD_ALU=:CODALU
       where m.codCARRE=:CARRE and m.cuatrim <=:max_cuat_check
       into :materias_anio, :materias_aprob;

       materias_aprob=coalesce(materias_aprob,0);
       if (ceiling(materias_anio/2)>materias_aprob) then begin
          FERRCOD=2;
          FERRMSG='Para inscribirse en una materia de este cuatrimestre debe tener la mitad mas 1 de materias aprobabas por final del a??o anterior';
          FERRWEB='Para inscribirse en una materia de este cuatrimestre debe tener la mitad mas 1 de materias aprobabas por final del a??o anterior';
       end

  end */
  IF (FERRCOD=0) THEN BEGIN --VERIFICO CORRELATIVIDADES
     CONDICION=NULL;
     IF (CORRELATI IS NOT NULL AND CORRELATI<>'' AND CORRELATI NOT CONTAINING '----') THEN
        BEGIN
          CONT=1; SIGO='S'; BANDERA='S';
          WHILE (CONT<=15 AND SIGO='S') DO
            BEGIN
              CODMATVER=SUBSTRING(CORRELATI FROM CONT FOR 2);
              CONDICION=NULL;
              IF (CODMATVER IS NOT NULL AND CODMATVER<>'') THEN
                BEGIN
                   IF (NOT EXISTS(SELECT 1 FROM ANALITIC A WHERE A.COD_ALU=:CODALU AND A.CARRE=:CARRE AND A.COD_MAT=:CODMATVER)) THEN
                     BEGIN
                          SELECT C.CONDICION
                          FROM CURSADA C
                          WHERE C.COD_ALU=:CODALU AND C.CARRE=:CARRE AND C.COD_MAT=:CODMATVER
                          INTO :CONDICION;
                          IF (CONDICION<>'REGULAR' OR CONDICION IS NULL) THEN
                            BEGIN
                               BANDERA='N';
                               SIGO='N';
                               SELECT TRIM(M.DESCRIPCI) FROM MATERIAS M WHERE M.CODMATERI=:CODMATVER AND M.CODCARRE=:CARRE INTO :MAT_ADEUDA;
                            END
                     END
                END
              CONT=CONT+3;
            END
          IF (BANDERA='N' AND TIPO='I') THEN
            BEGIN
              FERRCOD=2;
              FERRMSG=MATERIA||', adeuda correlatividades';
              FERRWEB='No se puede inscribir, adeudas correlatividades. <br> Primera materia adeudada: <br>'||coalesce(MAT_ADEUDA,'');
            END
        END
  END

SUSPEND;
end ^

SET TERM ; ^

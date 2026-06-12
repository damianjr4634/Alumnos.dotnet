SET TERM ^ ;

ALTER PROCEDURE XXX_EGRESADOS_V2 (CARRE VARCHAR(6) CHARACTER SET NONE,
DDE DATE,
HTA DATE,
CODALU VARCHAR(20) CHARACTER SET NONE)
RETURNS (COD_ALU VARCHAR(13) CHARACTER SET NONE,
NOMBRE VARCHAR(100) CHARACTER SET NONE,
FECHA DATE,
PROMEDIO NUMERIC(5, 2),
FECNAC DATE,
LUGARNAC VARCHAR(100) CHARACTER SET NONE,
MATRIZ VARCHAR(10) CHARACTER SET NONE,
TITULO VARCHAR(150) CHARACTER SET NONE,
CELULAR VARCHAR(100) CHARACTER SET NONE,
FEC_ING DATE)
AS 
declare variable ferrcod integer;
declare variable cuatri smallint;
declare variable cuatrim smallint;
declare variable carrecuat smallint;
declare variable carrecuat2 smallint;
declare variable carredes varchar(150);
declare variable carredes2 varchar(150);
declare variable sw smallint;
declare variable codmat char(2);
declare variable mat_equi char(2);
declare variable maxdate date;
BEGIN

INSERT INTO TMP_EGRE(COD_ALU, CARRE)
SELECT DISTINCT A.COD_ALU, A.CARRE
FROM ANALITIC A
--LEFT OUTER JOIN ALUMNOS S ON S.COD_ALU=A.COD_ALU AND S.CARRE=A.CARRE
WHERE A.CARRE=:CARRE AND A.FEC_FINAL BETWEEN :DDE AND :HTA
      and (a.cod_alu=:codalu or :codalu is null) ;--AND S.BAJA='N';

SELECT C.CUATRIM, C.CUATRIM2, C.DESCARRE, C.DESCARRE2
FROM CARRERA C
WHERE C.CARRE=:CARRE
INTO :CARRECUAT, :CARRECUAT2, :CARREDES, :CARREDES2;

FOR SELECT T.COD_ALU, TRIM(S.APELLIDO)||', '||TRIM(S.NOM_APE),
           S.FEC_NAC, S.MATRIZ, S.LUG_NAC, COALESCE(S.celular,S.tele), S.FEC_ING
FROM TMP_EGRE T
LEFT OUTER JOIN ALUMNOS S ON S.COD_ALU=T.COD_ALU AND S.CARRE=:CARRE
INTO :COD_ALU, :NOMBRE, :FECNAC, :MATRIZ, :LUGARNAC, :CELULAR, :FEC_ING
DO BEGIN
   SW=0;
   IF (COALESCE(CARRECUAT2,0)<>'0') THEN BEGIN
       FOR SELECT M.CODMATERI
       FROM MATERIAS M
       WHERE M.CODCARRE=:CARRE AND M.ESTADO<>'B' AND :SW=0 AND
             M.CUATRIM<=:CARRECUAT2
       ORDER BY M.CUATRIM
       INTO :CODMAT
       DO BEGIN
           IF (NOT EXISTS(SELECT 1
                          FROM ANALITIC C
                          WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:CODMAT)) THEN BEGIN
    
              SW=1;
           END
       END

       IF (SW=0) THEN BEGIN
          MAXDATE=NULL;
          SELECT MAX(C.FEC_FINAL)
          FROM ANALITIC C
          LEFT OUTER JOIN MATERIAS M ON M.CODMATERI=C.COD_MAT AND C.CARRE=M.CODCARRE
          WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND M.CUATRIM<=:CARRECUAT2
          INTO :MAXDATE;

          IF (MAXDATE BETWEEN :DDE AND :HTA) THEN BEGIN
             FECHA=MAXDATE;
             TITULO=CARREDES2;
             SELECT PROMGRAL FROM XXX_PROMEDIO_GRAL(:COD_ALU, :CARRE) INTO :PROMEDIO;
             SUSPEND;
          END
       END
   END
   IF (SW=0) THEN BEGIN
      FOR SELECT M.CODMATERI, trim(M.EQUIVALE)
       FROM MATERIAS M
       WHERE M.CODCARRE=:CARRE AND M.ESTADO<>'B' AND :SW=0 AND
             M.CUATRIM<=:CARRECUAT
       ORDER BY M.CUATRIM
       INTO :CODMAT, :MAT_EQUI
       DO BEGIN
           IF (:CARRE<>'ADM') THEN BEGIN
              IF ( NOT EXISTS(SELECT 1 FROM ANALITIC C
                              WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:CODMAT)) THEN BEGIN
                 IF ( NOT EXISTS(SELECT 1 FROM ANALITIC C
                                 WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:MAT_EQUI)) THEN BEGIN
                    SW=1;
                 END
              END
           END
           ELSE IF (:CARRE='ADM') THEN BEGIN
               IF (CODMAT NOT IN ('08','27','16','25','17','21') AND
                   NOT EXISTS(SELECT 1
                              FROM ANALITIC C
                              WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:CODMAT)) THEN BEGIN
                   SW=1;
               END
               ELSE BEGIN
                   IF (CODMAT IN ('08','27') AND
                       NOT EXISTS(SELECT 1
                                  FROM ANALITIC C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT IN ('08','27'))) THEN
                       SW=1;
                   IF (CODMAT IN ('16','25') AND
                       NOT EXISTS(SELECT 1
                                  FROM ANALITIC C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT IN ('16','25'))) THEN
                       SW=1;
                   IF (CODMAT IN ('17','21') AND
                       NOT EXISTS(SELECT 1
                                  FROM ANALITIC C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT IN ('17','21'))) THEN
                       SW=1;
               END
           END
       END

       IF (SW=0) THEN BEGIN
          MAXDATE=NULL;
          SELECT MAX(C.FEC_FINAL)
          FROM ANALITIC C
          LEFT OUTER JOIN MATERIAS M ON M.CODMATERI=C.COD_MAT AND C.CARRE=M.CODCARRE
          WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND M.CUATRIM<=:CARRECUAT
          INTO :MAXDATE;

          IF (MAXDATE BETWEEN :DDE AND :HTA) THEN BEGIN
             FECHA=MAXDATE;
             TITULO=CARREDES;
             SELECT PROMGRAL FROM XXX_PROMEDIO_GRAL(:COD_ALU, :CARRE) INTO :PROMEDIO;
             SUSPEND;
          END
       END

   END
END
DELETE FROM TMP_EGRE;
end ^

SET TERM ; ^

SET TERM ^ ;

ALTER PROCEDURE XXX_PARRAFO_CONSTANCIA (CODALU VARCHAR(11) CHARACTER SET NONE,
CARRE VARCHAR(6) CHARACTER SET NONE,
TIPO VARCHAR(10) CHARACTER SET NONE)
RETURNS (FPARRAFO VARCHAR(500) CHARACTER SET NONE)
AS 
declare variable NOMBRE varchar(80);
declare variable DOCUMENTO varchar(20);
declare variable CARRERA varchar(150);
declare variable DESCARRE2 varchar(150);
declare variable DESCARRE varchar(150);
declare variable CUAT smallint;
declare variable CUATRIM2 smallint;
declare variable FCUATRI smallint;
declare variable NOMMAT varchar(100);
declare variable FECFINAL date;
BEGIN
  SELECT TRIM(A.APELLIDO)||', '||TRIM(A.NOM_APE) FROM ALUMNOS A WHERE A.COD_ALU=:CODALU AND A.CARRE=:CARRE INTO :NOMBRE;
  DOCUMENTO=SUBSTRING(CODALU FROM 1 FOR 3)||'  '||SUBSTRING(CODALU FROM 4 FOR 2)||'.'||SUBSTRING(CODALU FROM 6 FOR 3)||'.'||SUBSTRING(CODALU FROM 9 FOR 3);
  SELECT C.DESCARRE||COALESCE(' '||C.DURACION,''), C.DESCARRE2, CUATRIM2, C.DESCARRE
  FROM CARRERA C WHERE C.CARRE=:CARRE INTO :CARRERA, :DESCARRE2, :CUATRIM2, :descarre;
  IF (TIPO='CTT') THEN BEGIN
     SELECT FCUATRI FROM XXX_IMPRIME_CTT(:CODALU,:CARRE) INTO :FCUATRI;
     IF (FCUATRI=COALESCE(CUATRIM2,0)) THEN
         FPARRAFO='           Hace constar que: '||UPPER(NOMBRE)||' - '||DOCUMENTO||', ha concluido el PRIMER CICLO de la carrera '||(SELECT FTEXTO FROM YYY_PASA_MAYUS(TRIM(:CARRERA)))||
                  ', habiendo obtenido el título intermedio de '||(SELECT FTEXTO FROM YYY_PASA_MAYUS(TRIM(:DESCARRE2)))||' (en trámite).-';
     ELSE IF (FCUATRI>COALESCE(CUATRIM2,0)) THEN BEGIN
         FPARRAFO='           Hace constar que: '||UPPER(NOMBRE)||' - '||DOCUMENTO||', ha concluido '||IIF(DESCARRE2 IS null, 'la carrera de ' ,'el PRIMER CICLO y SEGUNDO CICLO de la carrera ')||(SELECT FTEXTO FROM YYY_PASA_MAYUS(TRIM(:CARRERA)))||
                  ', habiendo obtenido el título completo (en trámite).-';
     END
  END
  ELSE IF (TIPO='PASE') THEN BEGIN
     CUAT=NULL;
     SELECT MAX(M.CUATRIM)
     FROM CURSADA C
     LEFT OUTER JOIN MATERIAS M ON M.CODCARRE=C.CARRE AND M.CODMATERI=C.COD_MAT
     WHERE CARRE=:CARRE AND COD_ALU=:CODALU AND C.CONDICION='REGULAR'
     INTO :CUAT;

     SELECT IIF(MAX(M.CUATRIM)>COALESCE(:CUAT,1),MAX(M.CUATRIM),COALESCE(:CUAT,1))
     FROM ANALITIC C
     LEFT OUTER JOIN MATERIAS M ON M.CODCARRE=C.CARRE AND M.CODMATERI=C.COD_MAT
     WHERE C.CARRE=:CARRE AND C.COD_ALU=:CODALU
     INTO :CUAT;

     FPARRAFO='           Hace constar que: '||UPPER(NOMBRE)||' - '||DOCUMENTO||
            ' del '||CUAT||' '||IIF(CARRE IN ('333','650'),'año','Cuat.')||', tiene en trámite su certificado de pase del ciclo '||(SELECT FTEXTO FROM YYY_PASA_MAYUS(TRIM(:CARRERA)))||'.';
  END
  ELSE IF (TIPO='ANALITICO') THEN BEGIN
     CUAT=NULL;
     SELECT MAX(M.CUATRIM)
     FROM CURSADA C
     LEFT OUTER JOIN MATERIAS M ON M.CODCARRE=C.CARRE AND M.CODMATERI=C.COD_MAT
     WHERE CARRE=:CARRE AND COD_ALU=:CODALU AND C.CONDICION='REGULAR'
     INTO :CUAT;

     SELECT IIF(MAX(M.CUATRIM)>COALESCE(:CUAT,1),MAX(M.CUATRIM),COALESCE(:CUAT,1))
     FROM ANALITIC C
     LEFT OUTER JOIN MATERIAS M ON M.CODCARRE=C.CARRE AND M.CODMATERI=C.COD_MAT
     WHERE C.CARRE=:CARRE AND C.COD_ALU=:CODALU
     INTO :CUAT;
     FPARRAFO='           Hace constar que: '||UPPER(NOMBRE)||' - '||DOCUMENTO||
               ' del '||CUAT||' Cuat., tiene en trámite su certificado analático de estudios completo '||(SELECT FTEXTO FROM YYY_PASA_MAYUS(TRIM(:CARRERA)))||'.';
  END
   ELSE IF (TIPO starting 'CE') THEN --COSTANCIA DE EXAMEN
  BEGIN
    SELECT M.descripci, A.fec_final
    FROM ANALITIC A
    LEFT OUTER JOIN MATERIAS M ON M.codmateri=A.cod_mat AND M.codcarre=A.carre
    WHERE A.cod_alu=:codalu AND A.carre=:CARRE AND A.cod_mat=REPLACE(:TIPO,'CE-','')
    INTO :NOMMAT, :FECFINAL;

    FPARRAFO='         Por la presente certificamos que '||UPPER(NOMBRE)||' - '||DOCUMENTO||' es alumno de la Carrera '||
             (SELECT FTEXTO FROM YYY_PASA_MAYUS(TRIM(:DESCARRE)))|| ' y ha rendido EXAMEN FINAL de la asignatura: '||
             (SELECT FTEXTO FROM YYY_PASA_MAYUS(TRIM(:NOMMAT)))  ||' el día '||LPAD(EXTRACT(day FROM :FECFINAL),2,'0')||'/'||LPAD(EXTRACT(MONTH FROM :FECFINAL),2,'0')||'/'||EXTRACT(YEAR FROM FECFINAL)||'.-'||ASCII_CHAR(13)||
             '         Se extiende la presente a pedido del interesado a los '||LPAD(EXTRACT(day FROM CURRENT_DATE),2,'0') ||' días del mes de '||
             CASE EXTRACT(MONTH FROM CURRENT_DATE)
                WHEN 1 THEN 'enero'
                WHEN 2 THEN 'febrero'
                WHEN 3 THEN 'marzo'
                WHEN 4 THEN 'abril'
                WHEN 5 THEN 'mayo'
                WHEN 6 THEN 'junio'
                WHEN 7 THEN 'julio'
                WHEN 8 THEN 'agosto'
                WHEN 9 THEN 'septiembre'
                WHEN 10 THEN 'octubre'
                WHEN 11 THEN 'noviembre'
                WHEN 12 THEN 'diciembre'
             END ||
             ' de '||EXTRACT(YEAR FROM CURRENT_DATE)||'.-';
  END
--exception e_custom_err fparrafo;
SUSPEND;
end ^

SET TERM ; ^

SET TERM ^ ;

ALTER PROCEDURE XXX_CARGA_FINAL (USUARIO SMALLINT,
CODALU VARCHAR(11) CHARACTER SET NONE)
RETURNS (FERRCOD INTEGER,
FERRMSG VARCHAR(100) CHARACTER SET NONE)
AS 
declare variable perm_exa integer;
declare variable final1 numeric(9,2);
declare variable fecha1 date;
declare variable final2 numeric(9,2);
declare variable fecha2 date;
declare variable final3 numeric(9,2);
declare variable fecha3 date;
declare variable matriz integer;
declare variable cod_alu char(11);
declare variable mesa integer;
declare variable carre varchar(6);
declare variable apellido varchar(30);
declare variable nom_ape varchar(30);
declare variable condicion varchar(15);
declare variable instituto varchar(30);
declare variable caracte varchar(6);
declare variable cod_mat char(2);
declare variable cua_anio integer;
declare variable notaana numeric(9,2);
declare variable fechaana date;
declare variable cond_ant varchar(30);
declare variable actfin1 varchar(10);
declare variable actfin2 varchar(10);
declare variable actfin3 varchar(10);
declare variable actfin varchar(10);
declare variable tipo varchar(3);
begin
  FERRCOD=0;
  FERRMSG='';
  FOR SELECT P.PERM_EXA,P.FINAL1,P.FECHA1,P.FINAL2,P.FECHA2,P.FINAL3,P.FECHA3,P.MATRIZ,
             P.COD_ALU, P.MESA, P.CARRE, P.APELLIDO, P.NOM_APE, P.CONDICION, P.COD_MAT,
             P.FACTFIN1, P.FACTFIN2,  P.FACTFIN3
  FROM "$$$PERMEXA" P
  WHERE P.USUARIO=:USUARIO
  INTO :PERM_EXA,:FINAL1,:FECHA1,:FINAL2,:FECHA2,:FINAL3,:FECHA3,:MATRIZ,
       :COD_ALU, :MESA, :CARRE, :APELLIDO, :NOM_APE, :CONDICION, :COD_MAT,
       :ACTFIN1, :ACTFIN2, :ACTFIN3
  DO BEGIN
      SELECT TIPO FROM CARRERA WHERE CARRE=:CARRE INTO :TIPO;
      IF (TIPO='TER') THEN
        BEGIN
         COND_ANT=NULL;
         SELECT CONDICION
         FROM CURSADA
         WHERE CARRE =:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT
         INTO :COND_ANT;
         UPDATE CURSADA C SET C.FINAL1=:FINAL1, C.FINAL2=:FINAL2, C.FINAL3=:FINAL3,
                              C.FECHA1=:FECHA1, C.FECHA2=:FECHA2, C.FECHA3=:FECHA3,
                              C.FACTFIN1=:ACTFIN1, C.FACTFIN2=:ACTFIN2, C.FACTFIN3=:ACTFIN3,
                              C.CONDICION=:CONDICION, USUARIO=:USUARIO
         WHERE C.COD_ALU=:COD_ALU AND C.CARRE=:CARRE AND C.COD_MAT=:COD_MAT;
         IF (CONDICION='FINAL') THEN
           BEGIN
              SELECT "INSTITUT", CARACT FROM CARRERA WHERE CARRE=:CARRE INTO :INSTITUTO,:CARACTE;
              SELECT CUA_ANIO FROM CURSADA WHERE CARRE=:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT INTO :CUA_ANIO;
              IF (FINAL1 >=4 AND FINAL1<=10) THEN
                BEGIN
                 NOTAANA=FINAL1;
                 FECHAANA=FECHA1;
                 ACTFIN=ACTFIN1;
                END
              IF (FINAL2 >=4 AND FINAL2<=10) THEN
                BEGIN
                 NOTAANA=FINAL2;
                 FECHAANA=FECHA2;
                 ACTFIN=ACTFIN2;
                END
              IF (FINAL3 >=4 AND FINAL3<=10) THEN
                BEGIN
                 NOTAANA=FINAL3;
                 FECHAANA=FECHA3;
                 ACTFIN=ACTFIN3;
                END
              IF (NOTAANA IS NOT NULL) THEN BEGIN
                 INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT, FACTFIN1,FACTFIN2,FACTFIN3)
                                  SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, :COND_ANT, FACTFIN1,FACTFIN2,FACTFIN3
                                  FROM CURSADA C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:COD_MAT;
                  DELETE FROM CURSADA C  WHERE C.COD_ALU=:COD_ALU AND C.CARRE=:CARRE AND C.COD_MAT=:COD_MAT;
                  INSERT INTO ANALITIC(COD_ALU,APELLIDO,COD_MAT,CUA_ANIO,NOTA_MAT,
                                     FEC_FINAL,MATRIZ,CONDICION,"INSTITUT",CARAC,ACTINT,ACTDGE,ACTSNE,
                                     CARRE,COLEGIO,"PLAN",A_C,NREG,USUARIO,FACTFIN)
                              VALUES(:COD_ALU, :APELLIDO,:COD_MAT,:CUA_ANIO,:NOTAANA,:FECHAANA,:MATRIZ,
                                     :CONDICION,:INSTITUTO,:CARACTE,NULL,NULL,NULL,:CARRE,NULL,NULL,NULL,NULL,:USUARIO,:ACTFIN);
              END
           END
        END
        IF (TIPO IN ('BAC','BAD')) THEN
          BEGIN
            COND_ANT=NULL;
            SELECT CONDICION
            FROM CURSADA
            WHERE CARRE =:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT
            INTO :COND_ANT;

            UPDATE CURSADA C SET C.FINAL1=:FINAL1,C.FECHA1=:FECHA1, C.CONDICION=:CONDICION,USUARIO=:USUARIO WHERE C.COD_ALU=:COD_ALU AND C.CARRE=:CARRE AND C.COD_MAT=:COD_MAT;
            IF (CONDICION IN ('REGULAR','LIBRE','FINAL')) THEN
             BEGIN
                SELECT "INSTITUT", CARACT FROM CARRERA WHERE CARRE=:CARRE INTO :INSTITUTO,:CARACTE;
                SELECT CUA_ANIO FROM CURSADA WHERE CARRE=:CARRE AND COD_ALU=:COD_ALU AND COD_MAT=:COD_MAT INTO :CUA_ANIO;
                IF (FINAL1 >=6 AND FINAL1<=10) THEN
                  BEGIN
                   NOTAANA=FINAL1;
                   FECHAANA=FECHA1;
                  END
                INSERT INTO CURSADA_HST (COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, CONDANT,FACTFIN1,FACTFIN2,FACTFIN3)
                                  SELECT COD_ALU, APELLIDO, CARRE, CUTUCO, COD_MAT, CUA_ANIO, TP_EVA, RECUP, TP_EVA2, RECUP2,
                                         REGULAR, TOT_HORAS, INASIST, JUSTIF, CONDICION, FINAL1, FECHA1, FINAL2, FECHA2,
                                         FINAL3, FECHA3, FINAL4, FECHA4, MATRIZ, INSTITUT, CARAC, ACTINT, ACTDGE, ACTSNE,
                                         NREG, COLEGIO, "PLAN", A_C, DEFINE, INDICE, TP_EVA3, PROM, USUARIO, FEC_EVA1,
                                         FEC_EVA2, FEC_EVA3, FAL_EVA1, FAL_EVA2, FAL_EVA3, :COND_ANT,FACTFIN1,FACTFIN2,FACTFIN3
                                  FROM CURSADA C
                                  WHERE C.CARRE=:CARRE AND C.COD_ALU=:COD_ALU AND C.COD_MAT=:COD_MAT;
                DELETE FROM CURSADA C  WHERE C.COD_ALU=:COD_ALU AND C.CARRE=:CARRE AND C.COD_MAT=:COD_MAT;
                INSERT INTO ANALITIC(COD_ALU,APELLIDO,COD_MAT,CUA_ANIO,NOTA_MAT,
                                     FEC_FINAL,MATRIZ,CONDICION,"INSTITUT",CARAC,ACTINT,ACTDGE,ACTSNE,
                                     CARRE,COLEGIO,"PLAN",A_C,NREG,USUARIO, FACTFIN)
                              VALUES(:COD_ALU, :APELLIDO,:COD_MAT,:CUA_ANIO,:NOTAANA,:FECHAANA,:MATRIZ,
                                     :CONDICION,:INSTITUTO,:CARACTE,NULL,NULL,NULL,:CARRE,NULL,
                                     NULL,NULL,NULL,:USUARIO,:ACTFIN);
             END
          END
  END
  /*SELECT FIRST 1 P.MESA, P.CARRE
  FROM "$$$PERMEXA" P
  WHERE P.USUARIO=:USUARIO
  INTO :MESA, :CARRE;*/
  DELETE FROM "$$$PERMEXA" WHERE USUARIO=:USUARIO;
--  DELETE FROM PERMEXA X WHERE X.MESA=:MESA AND X.CARRE=:CARRE AND X.COD_ALU=:CODALU;
  SUSPEND;
end ^

SET TERM ; ^

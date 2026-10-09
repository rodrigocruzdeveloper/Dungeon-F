using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonCreator : MonoBehaviour
{
    /*
    Este script cria automaticamente um ambiente 2D utilizando tilemap

    CRIAÇÃO LÓGICA:
    1 - Criar várias salas em posições aleatórias
    2 - Verificar se as salas não estão sobrepostas
    3 - Conexão das salas através de corredores

    CRIAÇÃO VISUAL:
    4 - Transformação das salas e corredores em tiles (chão)
    5 - Procurar as bordas do chão (seu entorno)
    6 - Colocar os tiles de paredes em volta do chão

    ATENÇÃO! 
    Não confunda aleatório com procedural
    Procedural trabalha com informações que podem ser replicadas

    DADOS (CRIANDO AS SALAS DE FORMA LÓGICA) -> REGRAS (VALIDAR AS SALAS) -> REPRESENTAÇÃO VISUAL (TILE)

    */


    /* 
       TILEMAPS SERA O GRID QUE IRA SUPORTAR O MAPA
    
        Floor - onde será desenho os tiles do chão
        Walls - onde será desenho os tiles da parede
    */

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;


    /*
        TILES
        
        Aqui informamos quais imagens (tile) serão utilizados.
        flooorTile - tile usado para o chão
        wallTile - tile usado para parede
    */
    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;


    /*
        CONFIGURAÇÃO DAS SALAS

        Quatidade de salas -> numberOfRooms = 15
        Significa que o algoritmo tentará criar 15 salas
    
        minRoomWidth = 6
        maxRoomWidth = 12
        Significa que uma sala poderá ter entre 6 e 12 tiles de largura

        O mesmo conceito é aplicado para a altura

    */
    [Header("Dungeon")]
    [SerializeField] private int numberOfRooms = 15;

    [SerializeField] private int minRoomWidth = 6;
    [SerializeField] private int maxRoomWidth = 12;

    [SerializeField] private int minRoomHeight = 6;
    [SerializeField] private int maxRoomHeight = 10;


    /*
        DISTANCIA ENTRE SALAS

        Esse valor é usado para evitar que duas salas sejam criadas muito proximas.

        Exemplo:
        roomSpacing = 3
    */
    [SerializeField] private int roomSpacing = 3;

    /*
        SEED
    
        A seed é um número utilizado para controlar a aleatoriedade.

        Esse valor garante que o algoritmo poderá replicar um resultado sempre que necessario.

        Seed 12345 -> Gera a Dungeon-A
        Sedd 12345 = (posso reutilizar) Dungeon-A
     
        Assim podemos reproduzir uma geração especifica.
    */
    [Header("Seed")]
    [SerializeField] private bool randomSeed = true;
    [SerializeField] private int seed = 12345;


    /*
        Estamos utilizando System.Random (C#) em vez de Random do Unity, porque queremos 
        controlar a seed
    */
    private System.Random random;


    /*
        LISTA DE SALAS

        Lista é uma estrutura de dados, que irá armazenar as caracteristicas das salas.

        RectInt -> é uma estrutura da Unity que representa um retangulo usando coordenadas inteiras

        Caracteristicas de uma sala:
        posicao X = 10
        posicao Y = 5

        largura = 8
        altura = 6

        RectInt(10, 5, 8, 6)

        Esta informação ainda não é visual, é uma informação lógica.
     */
    private List<RectInt> rooms = new List<RectInt>();


    /*
        POSICAO QUE SERAO O CHAO
        HashSet - È utilizado porque não queremos guarda a mesma posição várias vezes
        
        Exemplo:
        (5,5)
        (6,5)
        (7,5)
    
        Cada posição representa um tile no grid

     */
    private HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();


    // INICIA A CRIAÇÃO DAS SALAS
    private void Start()
    {
        GenerateDungeon();
    }

    /*
        METODO PRINCIPAL DE CRIACAO DAS DUNGEONS
      
        1 - Limpar a dungeon existente (caso houver)
        2 - Configurar a seed
        3 - Criar as salas (lógica) 
        4 - Conectar as salas através de corredores
        5 - Criar o chão
        6 - Criar as paredes
    */ 
    private void GenerateDungeon()
    {
        // ClearDungeon();

        // Se randomSeed estiver true, criamos uma seed aleatória
        if (randomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        
        // Criamos o gerador System.Random utilizando a seed escolhida
        random = new System.Random(seed);

        // Primeiro criamos as salas
        GenerateRooms();

        // Agora conectamos as salas validas atraves de corredores
        GenerateCorridors();


    }

   

    /*
        GENERATE ROOMS (CRIAR AS SALAS LOGICA)
        Não cria GameObjects.
        Apenas calcula onde as salas deverão existir.
    */
    private void GenerateRooms()
    {
        // Looping que executar a quantidade de salas que queremos criar (tentativas)
        for (int i = 0; i < numberOfRooms; i++)
        {
            // Caracteristicas da sala

            // Sortear a largura da sala 
            int width = random.Next(minRoomWidth, maxRoomWidth + 1);

            // A mesma coisa para altura sala
            int height = random.Next(minRoomHeight, maxRoomHeight + 1);

            /* 
                Agora sorteamos a posição da sala 
                A dungeon poderá se espalhar para coordenadas positivas e negativas             
            */
            int x = random.Next(-50, 51);
            int y = random.Next(-30, 31);

            // Criamos um RectInt para representar a sala
            RectInt newRoom = new RectInt(x, y, width, height);


            // Validar a sala verficando se ela não esta sobreposta a um existente
            bool overlaps = false;

            // Looping para verficiar se a nova sala esta sobrepondo alguma existente
            foreach (RectInt room in rooms)
            {
                // Criar uma margem em volta da sala que sera testada (nova sala)
                RectInt expandedRoom = new RectInt(
                        room.xMin - roomSpacing,
                        room.yMin - roomSpacing,
                        room.width + roomSpacing * 2,
                        room.height + roomSpacing * 2);

                // Verfica se dois retangulos se sobrepoem
                if (expandedRoom.Overlaps(newRoom))
                {
                    overlaps = true;
                    break;
                }
            }

            // Se a sala não entrou em conflito com nenhuma outra, podemos adicina-la a lista
            if (!overlaps)
            {
                rooms.Add(newRoom);
            }
        }
    }


    // CORREDORES

    /*
     * Depois de criar as salas, precisamos conecta-las
     * Para cada sala pegamos o centro dela e conectamos o centro da anterior
     * Isso cria uma estrutura simples de uma dungeon
     */

    private void GenerateCorridors()
    {
        // Começamos pela segunda sala porque a primeira não possui sala anterior
        for (int i = 1; i < rooms.Count; i++)
        {
            // SALA ANTERIOR - Pegamos o centro da sala anterior
            Vector2Int previousCenter = GetRoomCenter(rooms[i - 1]);

            // SALA ATUAL - Centro da sala atual
            Vector2Int currentCenter = GetRoomCenter(rooms[i]);

            // Sorteamos se o corredor começará na horizontal ou vertical
            // Isso gera pequenas variações na aparência dos corredores

            if(random.Next(0, 2) == 0)
            {
                // Horizontal


            }
            else
            {
                // Vertical
            }

        }


    }

    // GETROOMCENTER (ACHAR O CENTRO DAS SALAS)
    private Vector2Int GetRoomCenter(RectInt room)
    {
        return new Vector2Int(
            room.xMin + room.width / 2, 
            room.yMin + room.height / 2
            );
    }

    // CRIA CORREDOR HORIZONTAL
    private void CreateHorizontalCorridor(int startX, int endX, int y)
    {
        int min = Mathf.Min(startX, endX);

        int max = Mathf.Max(startX, endX);

        // Percorre a distancia min ate max
        for (int x = min; x <= max; x++) 
        {
            // Adicionar o primeiro tile do corredor
            floorPositions.Add(new Vector2Int(x, y));

            // Adcionamos o segundo tile
            floorPositions.Add(new Vector2Int(x, y + 1));
        }

       
    }

    // CONTINUAR AQUI!!!!!!


    // CRIAR O CORREDOR HORIZONTAL
    private void CreateVerticalCorridor(int startY, int endY, int x) 
    { 

    }



}

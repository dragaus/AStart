using System.Collections;
using System.Collections.Generic;

namespace PathFinding
{
    public enum PathFinderStatus
    {
        NOT_INITIALIZED,
        SUCCESS,
        FAILURE,
        RUNNING
    }

    //Esta es la clase de node
    //Esta nos va a servir para determinar el tipo de vertice y usarlo en el alagoritmo
    abstract public class Node<T>
    {
        public T Value { get; private set; }

        public Node(T value)
        {
            Value = value;
        }

        //Abstract de como obtener los nodos vecinos
        //Esta funcion va a ser super importante por que es la que determina
        //que nodos estan peagdos
        abstract public List<Node<T>> GetNeighbours();
    }

    //La clase path finder nos va a servior para poder implementar futuros algoritmos de busqueda
    abstract public class PathFinder<T>
    {

        #region Cost Calculation
        //Generamos una firma que nos permitara calcular el costo entre dos nodos
        // dependiendo el algoritmo
        public delegate float CostFunction(T a, T b);

        //Esta definira el costo de un nodo
        public CostFunction HeuristicCost { get; set; }
        //Esta cuanto nos costara viajar de un nodo a otro
        public CostFunction NodeTraversalCost { get; set; }
        #endregion

        //Va a revisar el estado actual de la implementacion
        //por defecto empezara en no inicializado
        #region Properties
        public PathFinderStatus Status
        {
            get;
            private set;
        } = PathFinderStatus.NOT_INITIALIZED;

        //Agregamos las propiedades de start y goal
        public Node<T> Start { get; private set; }
        public Node<T> Goal { get; private set;  }

        //Esta propiedad se encarga de guardar en donde se encuantra actualmente el Pathfinder
        public PathFinderNode CurrentNode { get; private set; }

        #endregion

        #region PathFinderNode
        // Clase PathFinderNode
        // esta clase ser encarga de crear todos los elementos necesarios para realizar la busqueda
        // no debe ser confundida con la clase Node
        // esta clase encapsula un Node y otros ativutos necesarios para la busqueda
        public class PathFinderNode
        {
            // El padre del nodo
            public PathFinderNode Parent { get; set; }

            // La ubbicacion en la que se encuentra el nodo
            public Node<T> Location { get; private set; }

            // Los diferentes costos
            public float Fcost { get; private set; }
            public float GCost { get; private set; }
            public float Hcost { get; private set; }

            // Un cosntructor
            // toma como valortes el nodo, el papa el gcost y el hcost
            public PathFinderNode(Node<T> location,
                PathFinderNode parent,
                float gCost,
                float hCost)
            {
                Location = location;
                Parent = parent;
                Hcost = hCost;
                SetGCost(gCost);
            }

            // Ajusta el valor de G y F
            public void SetGCost(float c)
            {
                GCost = c;
                Fcost = GCost + Hcost;
            }
        }
        #endregion

        #region Open and Closed lists and associated functions
        // Una lista abierta para el path.
        protected List<PathFinderNode> mOpenList = new List<PathFinderNode>();

        // La lista cerrada
        protected List<PathFinderNode> mClosedList = new List<PathFinderNode>();

        // Esta lista se enncarga de buscar el menor costo de la lista
        protected PathFinderNode GetLeastCostNode(List<PathFinderNode> myList)
        {
            int best_index = 0;
            float best_priority = myList[0].Fcost;
            for (int i = 1; i < myList.Count; i++)
            {
                if (best_priority > myList[i].Fcost)
                {
                    best_priority = myList[i].Fcost;
                    best_index = i;
                }
            }

            PathFinderNode n = myList[best_index];
            return n;
        }

        //Este metodo revisa si en nodo se encuentra en la lista,
        //si lo encuentra regresa su index
        //si no regresa -1
        protected int IsInList(List<PathFinderNode> myList, T cell)
        {
            for (int i = 0; i < myList.Count; ++i)
            {
                if (EqualityComparer<T>.Default.Equals(myList[i].Location.Value, cell))
                    return i;
            }
            return -1;
        }
        #endregion

        #region Delegates for action callbacks.
        // Estos son callbacks quue se van a utilizar para manejar cambios iunternos
        // se pueden usar para mostrar de manera grafica los cambios
        public delegate void DelegatePathFinderNode(PathFinderNode node);
        public DelegatePathFinderNode onChangeCurrentNode;
        public DelegatePathFinderNode onAddToOpenList;
        public DelegatePathFinderNode onAddToClosedList;
        public DelegatePathFinderNode onDestinationFound;

        public delegate void DelegateNoArgument();
        public DelegateNoArgument onStarted;
        public DelegateNoArgument onRunning;
        public DelegateNoArgument onFailure;
        public DelegateNoArgument onSuccess;
        #endregion

        #region PathFIndingSearch
        // Paso 1. Comenzar la busqueda
        // aqui comenzamos la busqueda
        // Aqui solo vamosd a poder iniciar la busqueda si no esta ejecuatndose la busqueda 
        public bool Initialize(Node<T> start, Node<T> goal)
        {
            if (Status == PathFinderStatus.RUNNING)
            {
                return false;
            }

            //Reiniciamos los valores
            Reset();

            // Ponemos los valores iniciales
            Start = start;
            Goal = goal;

            // Calculamos H desde el principio
            float H = HeuristicCost(Start.Value, Goal.Value);

            // Creamos una base que no tendra pare
            PathFinderNode root = new PathFinderNode(Start, null, 0f, H);

            // agregamos root a nuestra lista abierta
            mOpenList.Add(root);

            // ponemos el nodo actual en nuestro nodo inicial
            CurrentNode = root;

            // Llamamos los delgados si no estan definidos como nullos
            onChangeCurrentNode?.Invoke(CurrentNode);
            onStarted?.Invoke();

            // sponemos el status como RUNNING para evitar correr este metodos en estado sincronizado.
            Status = PathFinderStatus.RUNNING;

            return true;
        }

        // Paso 2: pasos hasta que encontremos una ruta o fallemos
        // Toma un paso de busqueda, este metodo se debe seguir ejecunatndo hatsa que
        // el estado sea SUCCESS o FAILURE
        public PathFinderStatus Step()
        {
            // Agrega el nodo actual a la lista de cerrados
            mClosedList.Add(CurrentNode);

            // Llama al delegadfo para aviusar a cualquier listener
            onAddToClosedList?.Invoke(CurrentNode);

            if (mOpenList.Count == 0)
            {
                // Si ya no quedan nodos abriertos se acabo nuestra busqueda y tenemos un error
                Status = PathFinderStatus.FAILURE;
                onFailure?.Invoke();
                return Status;
            }

            // Obtenemos el nodo menos costosos
            // el cual se convierte en nuestro nodo actual
            CurrentNode = GetLeastCostNode(mOpenList);

            // LLamamos al delagado si no esta definido como null.
            onChangeCurrentNode?.Invoke(CurrentNode);

            // Quitamos al nodo actual de la lista de abiertos.
            mOpenList.Remove(CurrentNode);

            // CRevusamos si el nodo actual contiene a goal
            // y si es asi regresamos SUCCESS
            if (EqualityComparer<T>.Default.Equals(
                CurrentNode.Location.Value, Goal.Value))
            {
                Status = PathFinderStatus.SUCCESS;
                onDestinationFound?.Invoke(CurrentNode);
                onSuccess?.Invoke();
                return Status;
            }

            // Encontramos a los nodos vecinos
            List<Node<T>> neighbours = CurrentNode.Location.GetNeighbours();

            // Buscamos los datos de los vecinos para saber que debemos hacer
            foreach (Node<T> cell in neighbours)
            {
                AlgorithmSpecificImplementation(cell);
            }

            Status = PathFinderStatus.RUNNING;
            onRunning?.Invoke();
            return Status;
        }

        abstract protected void AlgorithmSpecificImplementation(Node<T> cell);

        // Restablece los valores para una nueva busqueda
        protected void Reset()
        {
            if (Status == PathFinderStatus.RUNNING)
            {
                // No se puede restear los valores ya que estamos dentor de una busqueda
                return;
            }

            CurrentNode = null;

            mOpenList.Clear();
            mClosedList.Clear();

            Status = PathFinderStatus.NOT_INITIALIZED;
        }
        #endregion
    }

    #region AstarPathFinder
    // The A* Path Finder.
    public class AStarPathFinder<T> : PathFinder<T>
    {
        protected override void AlgorithmSpecificImplementation(Node<T> cell)
        {
            // Primero revismaos si el nodo se encuentra acualmenet en el closedList
            // si se encuentra ya no buscamos mas para ese nodo

            //Si no existe en la lista de cerrados
            if (IsInList(mClosedList, cell.Value) == -1)
            {

                //Calculamos el costo del nodo
                // Ruecuerada el G es el costo desde el inicio hasta ahora
                // Enonces para obtener G vamos a obtener el costo de G del currentNode
                // y agregar el costo dle nodo actual a la celda

                float G = CurrentNode.GCost + NodeTraversalCost(
                    CurrentNode.Location.Value, cell.Value);

                float H = HeuristicCost(cell.Value, Goal.Value);

                // Revisamos si la celda ya esta en open list
                int idOList = IsInList(mOpenList, cell.Value);
                //Si no existe en opern list
                if (idOList == -1)
                {
                    //Lo agregamos
                    PathFinderNode n = new PathFinderNode(cell, CurrentNode, G, H);
                    mOpenList.Add(n);
                    onAddToOpenList?.Invoke(n);
                }
                else
                {
                    // si la celda ya ewsta en open list
                    // revisamos si su costo es menor que el que hay en la lista
                    float oldG = mOpenList[idOList].GCost;
                    if (G < oldG)
                    {
                        //actualizamos el valor de G
                        mOpenList[idOList].Parent = CurrentNode;
                        mOpenList[idOList].SetGCost(G);
                        onAddToOpenList?.Invoke(mOpenList[idOList]);
                    }
                }
            }
        }
    }
    #endregion
}
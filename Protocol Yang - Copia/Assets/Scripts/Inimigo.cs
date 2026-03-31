using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Inimigo : MonoBehaviour
{
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private Rigidbody2D enemyrb;
    [SerializeField] private GameObject projetilPrefab;
    [SerializeField] private Transform pontoDisparo;
    [SerializeField] private float taxaDisparo = 2f;
    [SerializeField] private float distanciaPatrulha = 3f; // 3 passos
    [SerializeField] private float raioDeteccao = 7f; // Raio para detectar jogador
    
    private Vector3 posicaoInicial;
    private bool faceRight = true;
    private float tempoProximoDisparo;
    private Transform jogador;
    private bool emPerseguicao = false;

    private void Start()
    {
        posicaoInicial = transform.position;
        tempoProximoDisparo = taxaDisparo;
        
        // Tenta encontrar o jogador
        jogador = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        Debug.Log("Inimigo spawned em: " + posicaoInicial);
    }

    void Update()
    {
        // Detectar jogador no raio
        Collider2D[] colisoes = Physics2D.OverlapCircleAll(transform.position, raioDeteccao);
        emPerseguicao = false;
        foreach (Collider2D col in colisoes)
        {
            if (col.CompareTag("Player"))
            {
                emPerseguicao = true;
                break;
            }
        }

        if (emPerseguicao && jogador != null)
        {
            // Perseguir o jogador
            PerseguirJogador();
        }
        else
        {
            // Patrulhar
            Patrulhar();
        }
        
        // Disparo automático
        tempoProximoDisparo -= Time.deltaTime;
        if (tempoProximoDisparo <= 0)
        {
            Disparar();
            tempoProximoDisparo = taxaDisparo;
        }
    }

    private void Patrulhar()
    {
        // Movimento de patrulha limitado
        float direcao = faceRight ? 1f : -1f;
        float distanciaPercorrida = Mathf.Abs(transform.position.x - posicaoInicial.x);

        // Se atingiu a distância máxima, vira
        if (distanciaPercorrida >= distanciaPatrulha)
        {
            FlipEnemy();
        }

        enemyrb.linearVelocity = new Vector2(direcao * velocidade, enemyrb.linearVelocity.y);
    }

    private void PerseguirJogador()
    {
        if (jogador == null) return;

        float direcaoJogador = jogador.position.x > transform.position.x ? 1f : -1f;
        bool deveVirar = (direcaoJogador > 0 && !faceRight) || (direcaoJogador < 0 && faceRight);
        
        if (deveVirar)
        {
            FlipEnemy();
        }

        // Ficar parado e atirar (só zera movimento horizontal)
        enemyrb.linearVelocity = new Vector2(0, enemyrb.linearVelocity.y);
    }

    private void FlipEnemy()
    {
        faceRight = !faceRight;
        
        Vector3 escala = transform.localScale;
        escala.x *= -1;
        transform.localScale = escala;
    }

    private void Disparar()
    {
        if (projetilPrefab == null || pontoDisparo == null)
        {
            Debug.LogWarning("Prefab de projétil ou ponto de disparo não configurado!");
            return;
        }

        GameObject novoProjetil = Instantiate(projetilPrefab, pontoDisparo.position, Quaternion.identity);
        Projectile projectile = novoProjetil.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            Collider2D colisorInimigo = GetComponent<Collider2D>();
            projectile.ConfigurarOrigem(Projectile.OrigemProjetil.Inimigo, colisorInimigo);

            Vector2 direcao = faceRight ? Vector2.right : Vector2.left;
            projectile.Disparar(direcao);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision != null && !collision.collider.CompareTag("Player"))
        {
            // Só vira se não estiver perseguindo
            if (!emPerseguicao)
            {
                FlipEnemy();
            }
        }
    }
}

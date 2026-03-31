using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movimento : MonoBehaviour
{
    private float horizontalInput;
    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField] private int velocidade = 5;
    [SerializeField] private Transform Pe;
    [SerializeField] private LayerMask chaoLayer;
    [SerializeField] private GameObject projetilPrefab;
    [SerializeField] private Transform pontoDisparo;

    private bool EstaNoChao;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions["Move"];
        jumpAction = InputSystem.actions["Jump"];
    }

    private void Update()
    {
    horizontalInput = moveAction.ReadValue<Vector2>().x;

    if(jumpAction.WasPressedThisFrame() && EstaNoChao)
    {
        Debug.Log("Pulando!");
        rb.AddForce(Vector2.up * 150, ForceMode2D.Impulse);
    }
    
    if(Mouse.current.leftButton.wasPressedThisFrame)
    {
        Disparar();
    }
    
    EstaNoChao = Physics2D.OverlapCircle(Pe.position, 0.2f, chaoLayer);

}

private void FixedUpdate()
{
    rb.linearVelocity = new Vector2(horizontalInput * velocidade, rb.linearVelocity.y);
    
    // Virar o personagem de acordo com a direção
    if (horizontalInput > 0)
    {
        transform.localScale = new Vector3(1, 1, 1);
    }
    else if (horizontalInput < 0)
    {
        transform.localScale = new Vector3(-1, 1, 1);
    }
}

private void Disparar()
{
    if (projetilPrefab == null || pontoDisparo == null)
    {
        Debug.LogWarning("Prefab de projétil ou ponto de disparo não configurado!");
        return;
    }

    GameObject novoProjetil = Instantiate(projetilPrefab, pontoDisparo.position, Quaternion.identity);
    novoProjetil.transform.parent = null;
    Projectile projectile = novoProjetil.GetComponent<Projectile>();
    
    if (projectile != null)
    {
        Collider2D colisorJogador = GetComponent<Collider2D>();
        projectile.ConfigurarOrigem(Projectile.OrigemProjetil.Jogador, colisorJogador);

        Vector2 direcao = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        projectile.Disparar(direcao);
    }
}
}
